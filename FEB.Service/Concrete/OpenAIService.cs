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
        public async Task<ChatMessageContent> Ask(UserMessage userMessage)
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
            return await this.Ask(userMessage.Question, message.SessionKey);
        }
        public async Task<ChatMessageContent> Ask(string question, string sessionKey)
        {
            //var chatMessages = await _chatMessageService.GetChatMessages(sessionKey);
            var chatHistory = new ChatHistory();
            //foreach (var chatMessage in chatMessages)
            //{
            //    chatHistory.Add(new ChatMessageContent
            //    {
            //        Content = chatMessage.Message,
            //        Role = string.IsNullOrEmpty(chatMessage.UserID) ? AuthorRole.User : AuthorRole.Assistant
            //    });
            //}
            var questionVector = await Embed([question]);
            var relatedDocInfo=string.Empty;
            if (questionVector.Count > 0)
            {
                var relatedDocuments =await  _documentRepository.GetRelatedDocuments(questionVector[0].ToArray(), 2);
                foreach (var d in relatedDocuments)
                {
                    relatedDocInfo += d.DocumentChunk.Content;
                }
            }

            //OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
            //{
            //    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            //};
      

            // Add a system message to define the assistant's behavior
            chatHistory.AddSystemMessage("""
                You are a helpful AI assistant. Provide concise and accurate responses. 
                When responding, you may use the user's first name if available.
                Related Document content is:
                """+relatedDocInfo);
            
            chatHistory.AddUserMessage(question);

            var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
            var response = await chatCompletionService.GetChatMessageContentAsync(
                chatHistory,
                //openAIPromptExecutionSettings,
                kernel: _kernel
            );
            return response;
        }

        public async Task<IList<ReadOnlyMemory<float>>> Embed(List<string> chunks)
        {
            IList<ReadOnlyMemory<float>> embeddings =
                        await _textEmbeddingGenerationService.GenerateEmbeddingsAsync(chunks);
            return embeddings;
        }
    }
}
