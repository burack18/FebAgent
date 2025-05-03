using FEB.Infrastructure;
using FEB.Infrastructure.Concrete;
using FEB.Infrastructure.Dto;
using FEB.Infrastructure.Repositories.Abstract;
using FEB.Infrastructure.Repositories.Concrete;
using FEB.Service.Abstract;
using FEB.Service.Dto;
using FEBAgent.Domain;
using iText.Commons.Bouncycastle.Cert.Ocsp;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace FEB.Service.Concrete
{
    public class OpenAIService
    {
        private static Dictionary<string, ChatHistory> _chatHistory = new Dictionary<string, ChatHistory>();
        private static Dictionary<string, ChatHistory> _smartChatHistory = new Dictionary<string, ChatHistory>();

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
        public async Task<string> Ask(UserMessage userMessage, Constants.AgentService service)
        {

            var message = new ChatMessage
            {
                UserID = userMessage.UserID,
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

            if (chatHistory.Count > 20)
            {
                chatHistory.Clear();
            }


            //var enrichedQuestion = await EnrichQuestion(userMessage.Question, service);
            //var questionVectors = await Embed([.. enrichedQuestion, userMessage.Question]);
            var questionVectors = await Embed([userMessage.Question]);
            var relatedDocInfo = string.Empty;


            //List<RelatedDocument> relatedDocumentsCollection = [];
            //if (questionVectors.Count > 0)
            //{
            //    var tasks = questionVectors.Select(x => Task.Run(async () =>
            //    {
            //        var relatedDocuments = await _documentRepository.GetRelatedDocuments(x.ToArray(), 2);
            //        relatedDocumentsCollection.AddRange(relatedDocuments);
            //    }));
            //    await Task.WhenAll(tasks);
            //}
            //foreach (var d in relatedDocumentsCollection.DistinctBy(x => x.DocumentChunk.Id))
            //{
            //    relatedDocInfo += "\n" + d.DocumentChunk.Content;
            //}
            //var systemMessage = await _systemMessageRepository.GetSystemMessage();
            var systemMessage = new SystemMessage();

            //    var systemMessageFormatted = string.Format("""
            //{0}

            //You are a helpful AI assistant. You can use available functions and tools to answer user questions. Always use them when they can improve accuracy or provide helpful responses.
            //If a function provides the required information, use it without asking the user for unnecessary input. Only ask the user for parameters that are not already available or cannot be inferred.
            //If the user's identity or context is needed and a function can retrieve it, use that function instead of asking the user directly.

            //**Formatting Guidelines:**
            //* Format your response using Markdown syntax when appropriate to improve readability.
            //* Use bold (`**text**`) for emphasis on key terms, titles, or labels. Make sure the asterisks directly touch the word (`**Word**`, not `** Word **`).
            //* Use bullet points (`- item` or `* item`) for unordered lists. Start each item on a new line.
            //* Use numbered lists (`1. item`, `2. item`) for steps or ordered sequences. Start each item on a new line.
            //* **IMPORTANT List Separation:** Ensure there is a blank line *before* starting a bulleted or numbered list if it directly follows a paragraph or other text element. This separates the list visually and structurally.
            //* Format URLs as Markdown links: `[link text](URL)`.
            //* Use single backticks (`) for inline code or variable names if necessary.
            //* **IMPORTANT:** Do NOT wrap your entire response in Markdown code fences (like ```markdown ... ```). Only use code fences for actual blocks of code if requested or appropriate.
            //* Do NOT include the word 'markdown' at the start of your response.
            //* **Crucial Rule:** ALWAYS insert a blank line before starting any bulleted (`- ` or `* `) or numbered (`1. `) list. Do NOT put list items directly after other text without a blank line in between.

            //**Answering Process:**
            //1. Carefully read the user's question to understand what is being asked.
            //2. Thoroughly scan the provided "Related Document content" for relevant information.
            //3. Extract only the necessary details from the document to answer the question accurately.
            //4. Construct a clear and concise response using the extracted information ONLY.
            //5. Apply Markdown formatting (bolding, lists, links) where it enhances readability, following the guidelines above.


            //Related Document Content:
            //{1}
            //""", systemMessage?.Message ?? string.Empty, relatedDocInfo);
            //    chatHistory.AddSystemMessage(systemMessageFormatted);
            chatHistory.AddUserMessage(userMessage.Question);


            var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>(service.ToService());

            PromptExecutionSettings settings = PromptExecutionSettingsFactory.CreatePromptSettings(service);

            string aiMessage = string.Empty;

            ChatMessageContent response = settings == null
                                       ? await chatCompletionService.GetChatMessageContentAsync(chatHistory.SanitizeForGemini())
                                       : await chatCompletionService.GetChatMessageContentAsync(chatHistory, kernel: _kernel, executionSettings: settings);



            chatHistory.AddAssistantMessage(response?.Content ?? string.Empty);
            return response?.Content ?? string.Empty;
        }


        public async Task<string> AskSmart(UserMessage userMessage, Constants.AgentService service)
        {

            var message = new ChatMessage
            {
                UserID = userMessage.UserID,
                Message = userMessage.Question,
                TimestampCreated = DateTime.Now,
                SessionKey = userMessage.SessionKey ?? Guid.NewGuid().ToString(),
            };

            _chatMessageService.AddChatMessage(message);
            var hasKey = OpenAIService._smartChatHistory.ContainsKey(message.UserID);
            if (!hasKey)
            {
                OpenAIService._smartChatHistory.Add(message.UserID, new ChatHistory());
            }
            var chatHistory = OpenAIService._smartChatHistory[message.UserID];

            if (chatHistory.Count > 20)
            {
                chatHistory.Clear();
            }


            //var enrichedQuestion = await EnrichQuestion(userMessage.Question, service);
            //var questionVectors = await Embed([.. enrichedQuestion, userMessage.Question]);
            var questionVectors = await Embed([userMessage.Question]);
            var relatedDocInfo = string.Empty;


            List<RelatedDocument> relatedDocumentsCollection = [];
            if (questionVectors.Count > 0)
            {
                var tasks = questionVectors.Select(x => Task.Run(async () =>
                {
                    var relatedDocuments = await _documentRepository.GetRelatedDocuments(x.ToArray(), 2);
                    relatedDocumentsCollection.AddRange(relatedDocuments);
                }));
                await Task.WhenAll(tasks);
            }
            foreach (var d in relatedDocumentsCollection.DistinctBy(x => x.DocumentChunk.Id))
            {
                relatedDocInfo += "\n" + d.DocumentChunk.Content;
            }
            var systemMessage = await _systemMessageRepository.GetSystemMessage();

            var systemMessageFormatted = string.Format("""
        {0}

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


            var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>(service.ToService());

            PromptExecutionSettings settings = PromptExecutionSettingsFactory.CreatePromptSettings(service);

            string aiMessage = string.Empty;

            ChatMessageContent response = settings == null
                                       ? await chatCompletionService.GetChatMessageContentAsync(chatHistory.SanitizeForGemini())
                                       : await chatCompletionService.GetChatMessageContentAsync(chatHistory, kernel: _kernel, executionSettings: settings);



            chatHistory.AddAssistantMessage(response?.Content ?? string.Empty);
            return response?.Content ?? string.Empty;
        }


        public async Task<string> AskSmartStream(UserMessage userMessage,
            Constants.AgentService service,
            List<RelatedDocument> relatedDocumentsCollection
            )
        {

            var message = new ChatMessage
            {
                UserID = userMessage.UserID,
                Message = userMessage.Question,
                TimestampCreated = DateTime.Now,
                SessionKey = userMessage.SessionKey ?? Guid.NewGuid().ToString(),
            };

            _chatMessageService.AddChatMessage(message);
            var hasKey = OpenAIService._smartChatHistory.ContainsKey(message.UserID);
            if (!hasKey)
            {
                OpenAIService._smartChatHistory.Add(message.UserID, new ChatHistory());
            }
            var chatHistory = OpenAIService._smartChatHistory[message.UserID];

            if (chatHistory.Count > 20)
            {
                chatHistory.Clear();
            }


            //var enrichedQuestion = await EnrichQuestion(userMessage.Question, service);
            //var questionVectors = await Embed([userMessage.Question]);
            var relatedDocInfo = string.Empty;

            foreach (var d in relatedDocumentsCollection.DistinctBy(x => x.DocumentChunk.Id))
            {
                relatedDocInfo += "\n" + d.DocumentChunk.Content;
            }
            var systemMessage = await _systemMessageRepository.GetSystemMessage();

            var systemMessageFormatted = string.Format("""
        {0}

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


            var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>(service.ToService());

            PromptExecutionSettings settings = PromptExecutionSettingsFactory.CreatePromptSettings(service);

            string aiMessage = string.Empty;

            ChatMessageContent response = settings == null
                                       ? await chatCompletionService.GetChatMessageContentAsync(chatHistory.SanitizeForGemini())
                                       : await chatCompletionService.GetChatMessageContentAsync(chatHistory, kernel: _kernel, executionSettings: settings);



            chatHistory.AddAssistantMessage(response?.Content ?? string.Empty);
            return response?.Content ?? string.Empty;
        }

        public async Task<List<RelatedDocument>> GetRelatedDocuments(List<string> enrichedQuestion)
        {
            var questionVectors = await Embed([.. enrichedQuestion]);

            List<RelatedDocument> relatedDocumentsCollection = [];
            if (questionVectors.Count > 0)
            {
                var tasks = questionVectors.Select(x => Task.Run(async () =>
                {
                    var relatedDocuments = await _documentRepository.GetRelatedDocuments(x.ToArray(), 2);
                    relatedDocumentsCollection.AddRange(relatedDocuments);
                }));
                await Task.WhenAll(tasks);
            }

            return relatedDocumentsCollection;
        }

        public async Task<IList<ReadOnlyMemory<float>>> Embed(List<string> chunks)
        {
            IList<ReadOnlyMemory<float>> embeddings =
                        await _textEmbeddingGenerationService.GenerateEmbeddingsAsync(chunks);
            return embeddings;
        }

        public async Task<List<string>> EnrichQuestion(string question, Constants.AgentService service)
        {
            var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>(service.ToService());
            var history = new ChatHistory();

            history.AddSystemMessage("You are a helpful assistant that only returns JSON.");
            history.AddUserMessage($"""
                                    if the given question below is not meaningfull question or it is a greeting Return an empty JSON array of strings.
                                    Otherwise generate 5 specific, meaningful follow-up questions that could help clarify or expand on the original intent. Each question must start with "Who", "What", "When", "Where", "Why", or "How". Return only a JSON array of strings with no extra text.

                                    Question: "{question}"
                                    
                                    """);
            var response = await chatCompletionService.GetChatMessageContentAsync(
                history,
                kernel: _kernel);

            var enrichedQuestions = ExtractQuestionsFromResponse(response?.Content ?? string.Empty);

            return enrichedQuestions;
        }

        private List<string> ExtractQuestionsFromResponse(string response)
        {
            var match = Regex.Match(response, @"\[[\s\S]*?\]");

            if (match.Success)
            {
                try
                {
                    return JsonConvert.DeserializeObject<List<string>>(match.Value) ?? [];
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"JSON parse error: {ex.Message}");
                }
            }

            return new List<string>();
        }

        public async Task ClearChatHistory(string userID)
        {
            _ = _chatHistory.TryGetValue(userID, out ChatHistory history);
            if (history != null) history.Clear();

            _ = _smartChatHistory.TryGetValue(userID, out ChatHistory smartChatHistory);
            if (smartChatHistory != null) smartChatHistory.Clear();

            await Task.CompletedTask;
        }

        private struct ChatHistoryDto
        {
            public List<ChatMessage> Messages { get; set; }
            public DateTime TimestampCreated { get; set; }
        }
    }
}
