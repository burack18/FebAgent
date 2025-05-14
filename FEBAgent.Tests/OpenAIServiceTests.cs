using FEB.Infrastructure.Repositories.Abstract;
using FEB.Service.Abstract;
using FEB.Service.Concrete;
using FEB.Service.Dto;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel;
using System;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FEB.Infrastructure.Concrete;
using FEBAgent.Domain;
using Xunit;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace FEBAgent.Tests
{
    public class OpenAIServiceTests
    {
        private readonly Kernel _mockKernel;
        private readonly Mock<IChatMessageService> _mockChatMessageService;
        private readonly Mock<IChatCompletionService> _mockChatCompletionService;
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        private readonly Mock<ITextEmbeddingGenerationService> _mockTextEmbeddingGenerationService;
        private readonly Mock<IDocumentRepository> _mockDocumentRepository;
        private readonly Mock<ISystemMessageRepository> _mockSystemMessageRepository;
        private readonly OpenAIService _openAIService;

        public OpenAIServiceTests()
        {

            // Initialize mocks for all dependencies
            var kernelBuilder = Kernel.CreateBuilder();

            _mockChatMessageService = new Mock<IChatMessageService>();
            _mockChatCompletionService = new Mock<IChatCompletionService>();
            _mockTextEmbeddingGenerationService = new Mock<ITextEmbeddingGenerationService>();
            _mockDocumentRepository = new Mock<IDocumentRepository>();
            _mockSystemMessageRepository = new Mock<ISystemMessageRepository>();
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

            kernelBuilder.Services.AddSingleton(_mockChatCompletionService.Object);
            kernelBuilder.Services.AddKeyedSingleton("openai-chat", _mockChatCompletionService.Object); // Add this line
            kernelBuilder.Services.AddSingleton(_mockTextEmbeddingGenerationService.Object);

            _mockKernel = kernelBuilder.Build();

            // Create an instance of OpenAIService with the mocked dependencies
            _openAIService = new OpenAIService(
                _mockKernel,
                _mockChatMessageService.Object,
                _mockTextEmbeddingGenerationService.Object,
                _mockDocumentRepository.Object,
                _mockSystemMessageRepository.Object
            );
        }

        [Fact]
        public async Task Ask_ValidUserMessage_ReturnsAiResponseAndAddsChatMessage()
        {
            // Arrange
            var userMessage = new UserMessage { UserID = "user123", Question = "What is the capital of Belgium?", SessionKey = "session456" };
            var service = Constants.AgentService.OPENAI;
            var expectedResponse = "Brussels is the capital of Belgium.";
            var mockChatMessageContent = new ChatMessageContent(AuthorRole.Assistant, expectedResponse);

            var chatHistory = new ChatHistory();
            chatHistory.AddUserMessage(userMessage.Question);

            _mockChatCompletionService
                 .Setup(c => c.GetChatMessageContentsAsync(
                     It.IsAny<ChatHistory>(),
                     It.IsAny<PromptExecutionSettings>(),
                     _mockKernel,
                     It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<ChatMessageContent> { mockChatMessageContent });

            // Act
            var actualResponse = await _openAIService.Ask(userMessage, service);

            // Assert
            Assert.Equal(expectedResponse, actualResponse);

            _mockChatMessageService.Verify(c => c.AddChatMessage(It.Is<ChatMessage>(m =>
                m.UserID == userMessage.UserID &&
                m.Message == userMessage.Question &&
                m.SessionKey == userMessage.SessionKey
                )), Times.Once);

  
        }
    }
}