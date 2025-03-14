using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace FEBAgent.Service
{
    public class OpenAIService
    {
        private Kernel _kernel;
        public OpenAIService(Kernel kernel)
        {
            _kernel = kernel;
        }
        public async Task<ChatMessageContent> Ask(string question)
        {
            var chatHistory = new ChatHistory();
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

    }
}
