using Azure.Storage;
using Azure.Storage.Blobs;
using FEB.Infrastructure;
using FEB.Infrastructure.Repositories.Abstract;
using FEB.Infrastructure.Repositories.Concrete;
using FEB.Service.Abstract;
using FEB.Service.Concrete;
using FEB.Service.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.Connectors.Google;

namespace FEB.Service
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddService(this IServiceCollection services, IConfiguration config)
        {
            var blobKey = config
            .GetSection("BlobStorage").GetValue<string>("Key");
            string api_key = config
                .GetSection("AppSettings")
                .GetValue<string>("ApiKey") ?? throw new Exception("ApiKey Required");
            string gemini_key = config
                            .GetSection("AppSettings")
                            .GetValue<string>("GeminiKey") ?? throw new Exception("ApiKey Required");
            var conn = config
                        .GetSection("BlobStorage").GetValue<string>("ConnectionString");
            var blobUrl = $"https://febagent.blob.core.windows.net";
            var credentials = new StorageSharedKeyCredential("febagent", blobKey);

            services.AddSingleton(provider =>
                new Lazy<IDocumentService>(() => provider.GetRequiredService<IDocumentService>()));
            services.AddSingleton<IDocumentService, DocumentStorage.DocumentStorage>();
            services.AddSingleton<IDocumentRepository, DocumentRepository>();
            services.AddSingleton<IChatMessageRepository, ChatMessageRepository>();
            services.AddSingleton<IChatMessageService, ChatMessageService>();
            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<ISystemMessageService, SystemMessageService>();
            services.AddSingleton<IAIPlugin, OpenAIPlugin>();
            services.AddSingleton<IOpenAIService, OpenAIService>();

            services.AddOpenAIChatCompletion(
                modelId: "gpt-4o-mini", 
                apiKey: api_key, 
                serviceId: "openai-chat");
#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            services.AddOpenAITextEmbeddingGeneration(
                modelId: "text-embedding-ada-002",
                apiKey: api_key
            );

#pragma warning restore SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

#pragma warning disable SKEXP0070
            services.AddGoogleAIGeminiChatCompletion(
                    modelId: "gemini-2.0-flash",
                    apiKey: gemini_key,
                    serviceId: "gemini-chat"
                );
#pragma warning restore SKEXP0070

            services.AddTransient((serviceProvider) =>
            {
                var kernel = new Kernel(serviceProvider);

                var plugin = kernel.CreatePluginFromType<OpenAIPlugin>("OpenAIPlugin");
                kernel.Plugins.Add(plugin);
                return kernel;
            });


            services.AddSingleton(new BlobServiceClient(conn));
            return services;
        }
    }
}
