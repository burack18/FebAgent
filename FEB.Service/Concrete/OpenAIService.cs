using FEB.Infrastructure.Repositories.Abstract;
using FEB.Infrastructure.Repositories.Concrete;
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
        private readonly ISystemMessageRepository _systemMessageRepository;

        public OpenAIService(Kernel kernel, IChatMessageService chatMessageService, ITextEmbeddingGenerationService textEmbeddingGenerationService, IDocumentRepository documentRepository, ISystemMessageRepository systemMessageRepository)
        {
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            _kernel = kernel;
            _chatMessageService = chatMessageService;
            _textEmbeddingGenerationService = textEmbeddingGenerationService;
            _documentRepository = documentRepository;
            _systemMessageRepository = systemMessageRepository;
        }
        public async Task<string> Ask(UserMessage userMessage)
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
            var systemMessage = await _systemMessageRepository.GetSystemMessage();
            var systemMessageFormatted = string.Format("""
        {{0}}

        You are a helpful AI assistant. You can use available functions and tools to answer user questions. Always use them when they can improve accuracy or provide helpful responses.
        If a function provides the required information, use it without asking the user for unnecessary input. Only ask the user for parameters that are not already available or cannot be inferred.
        If the user's identity or context is needed and a function can retrieve it, use that function instead of asking the user directly.
        
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
        {1}
        """, systemMessage?.Message ?? string.Empty, relatedDocInfo);
            chatHistory.AddSystemMessage(systemMessageFormatted);
            chatHistory.AddUserMessage(userMessage.Question);

            var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

            OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            // No buffer needed for non-streaming
            // string buffer = ""; 
            string aiMessage = ""; // Accumulate the full response here
                                   // No yield-related variables needed
                                   // char[] yieldChars = ...
                                   // int yieldLengthThreshold = ...

            var response = await chatCompletionService.GetChatMessageContentAsync(chatHistory, kernel: _kernel, executionSettings: openAIPromptExecutionSettings);

            chatHistory.AddAssistantMessage(response?.Content ?? string.Empty);
            return response?.Content ?? string.Empty;
        }

        public async Task<IList<ReadOnlyMemory<float>>> Embed(List<string> chunks)
        {
            IList<ReadOnlyMemory<float>> embeddings =
                        await _textEmbeddingGenerationService.GenerateEmbeddingsAsync(chunks);
            return embeddings;
        }
    }
}
