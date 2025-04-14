using FEB.Infrastructure.Repositories.Abstract;
using FEB.Service.Abstract;
using FEB.Service.Dto;
using FEBAgent.Domain;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace FEB.Service.Concrete
{
    public class OpenAIService
    {
        private static Dictionary<string, ChatHistory> _chatHistory = new Dictionary<string, ChatHistory>();
        private Kernel _kernel;
        private IChatMessageService _chatMessageService;
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        private ITextEmbeddingGenerationService _textEmbeddingGenerationService;
        private readonly IDocumentRepository _documentRepository;

        public OpenAIService(Kernel kernel, IChatMessageService chatMessageService, ITextEmbeddingGenerationService textEmbeddingGenerationService, IDocumentRepository documentRepository)
        {
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            _kernel = kernel;
            _chatMessageService = chatMessageService;
            _textEmbeddingGenerationService = textEmbeddingGenerationService;
            _documentRepository = documentRepository;
        }
        public async IAsyncEnumerable<string> Ask(UserMessage userMessage)
        {
            //var isSessionExpired = await _chatMessageService.IsSessionExpired(userMessage.SessionKey);

            //if (isSessionExpired) throw new Exception("Session expired");



            var message = new ChatMessage
            {
                Message = userMessage.Question,
                TimestampCreated = DateTime.Now,
                SessionKey = userMessage.SessionKey ?? Guid.NewGuid().ToString(),
            };

            _chatMessageService.AddChatMessage(message);
            var hasKey = OpenAIService._chatHistory.ContainsKey(message.UserID);
            if (!hasKey)
            {
                OpenAIService._chatHistory.Add(message.UserID, new ChatHistory());
            }

            var chatHistory = OpenAIService._chatHistory[message.UserID];

            var questionVector = await Embed([userMessage.Question]);
            var relatedDocInfo = string.Empty;

            if (questionVector.Count > 0)
            {
                var relatedDocuments = await _documentRepository.GetRelatedDocuments(questionVector[0].ToArray(), 3);
                foreach (var d in relatedDocuments)
                {
                    relatedDocInfo += "\n" + d.DocumentChunk.Content;
                }
            }

            chatHistory.AddSystemMessage("""
        You are a helpful AI assistant. Your primary goal is to answer user questions based *only* on the provided "Related Document content". Do not use any external knowledge or make assumptions beyond the provided text.

        **Formatting Guidelines:**
        * Format your response using Markdown syntax when appropriate to improve readability.
        * Use bold (`**text**`) for emphasis on key terms, titles, or labels. Make sure the asterisks directly touch the word (`**Word**`, not `** Word **`).
        * Use bullet points (`- item` or `* item`) for unordered lists. Start each item on a new line.
        * Use numbered lists (`1. item`, `2. item`) for steps or ordered sequences. Start each item on a new line.
        * **IMPORTANT List Separation:** Ensure there is a blank line *before* starting a bulleted or numbered list if it directly follows a paragraph or other text element. This separates the list visually and structurally.
        * Format URLs as Markdown links: `[link text](URL)`.
        * Use single backticks (`) for inline code or variable names if necessary.
        * **IMPORTANT:** Do NOT wrap your entire response in Markdown code fences (like ```markdown ... ```). Only use code fences for actual blocks of code if requested or appropriate.
        * Do NOT include the word 'markdown' at the start of your response.
        * **Crucial Rule:** ALWAYS insert a blank line before starting any bulleted (`- ` or `* `) or numbered (`1. `) list. Do NOT put list items directly after other text without a blank line in between.

        **Answering Process:**
        1. Carefully read the user's question to understand what is being asked.
        2. Thoroughly scan the provided "Related Document content" for relevant information.
        3. Extract only the necessary details from the document to answer the question accurately.
        4. Construct a clear and concise response using the extracted information ONLY.
        5. Apply Markdown formatting (bolding, lists, links) where it enhances readability, following the guidelines above.
        
        Related Document Content:
        """ + relatedDocInfo);

            chatHistory.AddUserMessage(userMessage.Question);

            var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

            string buffer = "";
            string aiMessage = "";
            // Define characters that indicate a reasonable place to break the stream chunk
            char[] yieldChars = [' ', '\n', '\r', '\t', '.', ',', ';', ':', '!', '?', ')', ']']; // Added more punctuation
            int yieldLengthThreshold = 80; // Adjust as needed: Yield if buffer gets this long without a natural break

            await foreach (var content in chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory, kernel: _kernel))
            {
                if (string.IsNullOrEmpty(content.Content)) continue;

                // IMPORTANT: Make sure any necessary prefix removal (like "markdown ")
                // happens HERE to content.Content before adding to buffer, if still needed.
                string currentPart = content.Content;
                // Example prefix removal (add if needed):
                // if (currentPart.StartsWith("markdown ")) { currentPart = currentPart.Substring("markdown ".Length); }

                buffer += currentPart;
                aiMessage += currentPart;


                // Process the buffer to yield chunks ending at natural breaks
                while (buffer.Length > 0) // Keep processing buffer until it's empty or no break found
                {
                    int breakIndex = buffer.LastIndexOfAny(yieldChars);

                    // Option 1: Found a natural break point somewhat recently
                    // We check breakIndex > 0 to avoid yielding just punctuation/whitespace
                    if (breakIndex > 0 && breakIndex < buffer.Length - 1)
                    {
                        // Yield up to and including the break character
                        string partToYield = buffer.Substring(0, breakIndex + 1);
                        buffer = buffer.Substring(breakIndex + 1); // Keep the remainder
                        if (!string.IsNullOrEmpty(partToYield))
                        {
                            yield return partToYield;
                        }
                    }
                    // Option 2: Buffer is long, force yield (split at threshold, prefer not breaking words if possible)
                    else if (buffer.Length >= yieldLengthThreshold)
                    {
                        // Try to find a space near the threshold to avoid breaking mid-word
                        int splitPoint = buffer.LastIndexOf(' ', yieldLengthThreshold - 1);
                        if (splitPoint <= 0) // No space found near threshold, just split at threshold
                        {
                            splitPoint = yieldLengthThreshold;
                        }

                        string partToYield = buffer.Substring(0, splitPoint);
                        buffer = buffer.Substring(splitPoint); // Keep the remainder
                        if (!string.IsNullOrEmpty(partToYield))
                        {
                            yield return partToYield;
                        }
                    }
                    // Option 3: No natural break found yet and buffer is not too long, wait for more content
                    else
                    {
                        break; // Exit the while loop and wait for the next chunk from the AI
                    }
                }
            }

            // Yield any remaining content in the buffer after the AI stream finishes
            if (buffer.Length > 0)
            {
                yield return buffer;
            }
            chatHistory.AddAssistantMessage(aiMessage);
        }

        public async Task<IList<ReadOnlyMemory<float>>> Embed(List<string> chunks)
        {
            IList<ReadOnlyMemory<float>> embeddings =
                        await _textEmbeddingGenerationService.GenerateEmbeddingsAsync(chunks);
            return embeddings;
        }
    }
}
