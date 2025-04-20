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
using FEB.Infrastructure.Repositories.Abstract;
using FEB.Infrastructure.Repositories.Concrete;
using Microsoft.SemanticKernel.Connectors.OpenAI;

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

            services.AddSingleton<IUserRepository, UserRepository>();
            services.AddSingleton<ISystemMessageRepository, SystemMessageRepository>();

            
            services.AddSingleton((provider) =>
            {


                var options = new CosmosClientOptions()
                {
                    ApplicationName = cosmosDbSettings.DatabaseName
                };
                var cosmosClient = new CosmosClient(cosmosDbSettings.AccountEndpoint, cosmosDbSettings.AccountKey, options);
                return cosmosClient;

            });
        


            return services;
        }
    }
}
