using Xunit;
using FEB.Infrastructure.Concrete;
using FEB.Service.Concrete;
using FEBAgent.Domain;
using Moq;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Threading.Tasks;
using FEB.Infrastructure.Repositories.Concrete;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FEB.Infrastructure.Configuration;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Linq;
using FEB.Infrastructure.Repositories.Abstract;
using FEB.Service.Dto;
using Microsoft.SemanticKernel.Embeddings;
using FEB.Infrastructure;
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

namespace FEBAgent.Tests.IntegrationTests
{
    public class FEBAgentIntegrationTests : IDisposable
    {
        private readonly ChatMessageService _chatMessageService;
        private readonly OpenAIService _openAIService;
        private readonly Kernel _kernel;
        private readonly ChatMessageRepository _chatMessageRepository;
        private readonly CosmosClient _cosmosClient;
        private readonly IConfigurationManager _configuration;
        private readonly FebAgentContext _dbContext; // Add this

        public FEBAgentIntegrationTests()
        {
            // Build configuration
            var configManager = new ConfigurationManager()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json");

            _configuration = (IConfigurationManager)configManager.Build();

            var cosmosDbSettings = _configuration
                .GetSection("CosmosDb")
                .Get<CosmosDbSettings>() ?? throw new Exception("CosmosDb Config is required");

            // Setup Cosmos Client
            _cosmosClient = new CosmosClient(cosmosDbSettings.AccountEndpoint, cosmosDbSettings.AccountKey);

            // Initialize an instance of your custom DbContext
            _dbContext = new FebAgentContext();

            // Initialize repositories
            _chatMessageRepository = new ChatMessageRepository(_dbContext, _configuration);

            // Initialize services, injecting the repository
            _chatMessageService = new ChatMessageService(_chatMessageRepository);
            var kernelBuilder = Kernel.CreateBuilder();
            kernelBuilder.Services.AddSingleton(new Mock<IChatCompletionService>().Object);
            var blobKey = _configuration
                    .GetSection("BlobStorage").GetValue<string>("Key");
            string api_key = _configuration
                .GetSection("AppSettings")
                .GetValue<string>("ApiKey");
            var kernel = Kernel.CreateBuilder()
                        .AddOpenAIChatCompletion(
                            modelId: "gpt-4o-mini",
                            apiKey: api_key,
                            serviceId: "openai-chat"
                        )
                        .Build();
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            _openAIService = new OpenAIService(_kernel, _chatMessageService, new Mock<ITextEmbeddingGenerationService>().Object, new Mock<IDocumentRepository>().Object, new Mock<ISystemMessageRepository>().Object);
#pragma warning restore SKEXP0001
        }

        // Helper method to ensure the Cosmos DB database and container are created
        private async Task EnsureCosmosDbSetup()
        {
            var cosmosDbSettings = _configuration
                .GetSection("CosmosDb")
                .Get<CosmosDbSettings>() ?? throw new Exception("CosmosDb Config is required");

            await _cosmosClient.CreateDatabaseIfNotExistsAsync(cosmosDbSettings.DatabaseName);
            var db = _cosmosClient.GetDatabase(cosmosDbSettings.DatabaseName);
            await db.CreateContainerIfNotExistsAsync("messages", "/SessionKey");
        }

        [Fact]
        public async Task Test_AddChatMessage_Integration()
        {
            // Arrange
            var userMessage = new UserMessage
            {
                UserID = "user1",
                Question = "Hello, how are you?",
                SessionKey = "session1"
            };
            var chatMessage = new ChatMessage
            {
                UserID = userMessage.UserID,
                Message = userMessage.Question,
                SessionKey = userMessage.SessionKey,

            };

            // Act
            _chatMessageService.AddChatMessage(chatMessage);
            // With your current DbContext, SaveChangesAsync doesn't do anything.
            // The ChatMessageRepository likely adds directly to the _dbContext.ChatMessages list.

            // Assert
            var savedMessage = _dbContext.ChatMessages.FirstOrDefault(m => m.SessionKey == userMessage.SessionKey);
            Assert.NotNull(savedMessage);
            Assert.Equal(userMessage.Question, savedMessage.Message);
            Assert.Equal(userMessage.SessionKey, savedMessage.SessionKey);
        }

        [Fact]
        public async Task Test_OpenAIService_Ask_Integration()
        {
            // Arrange
            await EnsureCosmosDbSetup();
            var userMessage = new UserMessage { UserID = "user123", Question = "What is the capital of Belgium?", SessionKey = "session456" };
            var service = Constants.AgentService.OPENAI;


            // Override the service registered in the Kernel
            var kernelBuilder = Kernel.CreateBuilder();
            string api_key = _configuration
           .GetSection("AppSettings")
           .GetValue<string>("ApiKey");
            var kernel = Kernel.CreateBuilder()
                        .AddOpenAIChatCompletion(
                            modelId: "gpt-4o-mini",
                            apiKey: api_key,
                            serviceId: "openai-chat"
                        )
                        .Build();
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

            // Create OpenAIService with the mocked Kernel
            var openAIService = new OpenAIService(kernel, _chatMessageService, new Mock<ITextEmbeddingGenerationService>().Object, new Mock<IDocumentRepository>().Object, new Mock<ISystemMessageRepository>().Object);

            // Act
            var actualResponse = await openAIService.Ask(userMessage, service);

            // Assert
            Assert.True(!string.IsNullOrEmpty(actualResponse));
        }

        public void Dispose()
        {
            _cosmosClient?.Dispose();
            // No need to dispose _dbContext as it's just an in-memory list in your current implementation.
        }
    }
}