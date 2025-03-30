using FEB.Infrastructure.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;

namespace FEB.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastucture(this IServiceCollection services, IConfiguration configuration)
        {
            // Add services to the container.
            string api_key = configuration
                .GetSection("AppSettings")
                .GetValue<string>("ApiKey") ?? throw new Exception("ApiKey Required");
            var cosmosDbSettings = configuration
                .GetSection("CosmosDb")
                .Get<CosmosDbSettings>() ?? throw new Exception("CosmosDb Config is required");

            services.AddSingleton((provider) =>
            {


                var options = new CosmosClientOptions()
                {
                    ApplicationName = cosmosDbSettings.DatabaseName
                };
                var cosmosClient = new CosmosClient(cosmosDbSettings.AccountEndpoint, cosmosDbSettings.AccountKey, options);
                return cosmosClient;

            });
            services.AddOpenAIChatCompletion(modelId: "gpt-4o-mini", apiKey: api_key);
        #pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            services.AddOpenAITextEmbeddingGeneration(
                modelId: "text-embedding-ada-002",       
                apiKey: api_key
            );
        #pragma warning restore SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

            services.AddTransient((serviceProvider) =>
            {

                return new Kernel(serviceProvider);
            });
            return services;
        }
    }
}
