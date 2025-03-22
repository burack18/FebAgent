using FEB.Infrastructure.Repositories.Abstract;
using FEB.Service.Abstract;
using FEB.Service.Dto;
using FEBAgent.Domain;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace FEB.Service.Concrete
{
    public class OpenAIService
    {
        private Kernel _kernel;
        private IChatMessageService _chatMessageService;

        public OpenAIService(Kernel kernel, IChatMessageService chatMessageService)
        {
            _kernel = kernel;
            _chatMessageService = chatMessageService;
        }
        public async Task<ChatMessageContent> Ask(UserMessage userMessage)
        {
            var isSessionExpired = await _chatMessageService.IsSessionExpired(userMessage.SessionKey);

            if (isSessionExpired) throw new Exception("Session expired");


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
            var chatMessages = await _chatMessageService.GetChatMessages(sessionKey);
            var chatHistory = new ChatHistory();
            foreach (var chatMessage in chatMessages)
            {
                chatHistory.Add(new ChatMessageContent
                {
                    Content = chatMessage.Message,
                    Role = string.IsNullOrEmpty(chatMessage.UserID) ? AuthorRole.User : AuthorRole.Assistant
                });
            }

            OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };
            string userInfo = """
                {
                "firstName":"omer faruk"
                }
                """;

            // Add a system message to define the assistant's behavior
            chatHistory.AddSystemMessage("""
                You are a helpful AI assistant. Provide concise and accurate responses. 
                When responding, you may use the user's first name if available.
                """ + userInfo);
            chatHistory.AddUserMessage(question);

            var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
            var response = await chatCompletionService.GetChatMessageContentAsync(
                chatHistory,
                openAIPromptExecutionSettings,
                kernel: _kernel
            );
            return response;
        }

        public async Task<string> Embed()
        {
            throw new NotImplementedException();
        }
    }
}
