using FEB.Infrastructure.Configuration;
using FEB.Infrastructure.Repositories.Abstract;
using FEBAgent.Domain;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEB.Infrastructure.Repositories.Concrete
{
    public class SystemMessageRepository : ISystemMessageRepository
    {
        private CosmosClient _client;
        private Container _systemMessageContainer;
        private Container _chunkContainer;

        public SystemMessageRepository(CosmosClient client, IConfiguration configuration)
        {
            var cosmosConfig = configuration
                .GetSection("CosmosDb")
                .Get<CosmosDbSettings>() ?? throw new Exception("CosmosDB configuration Required");
            _client = client;
            _systemMessageContainer = _client.GetContainer(cosmosConfig.DatabaseName, "systemMessages");
            _chunkContainer = _client.GetContainer(cosmosConfig.DatabaseName, "chunks");
        }
        public async Task AddSystemMessage(SystemMessage systemMessage)
        {
            systemMessage.Id = Guid.NewGuid().ToString();
            systemMessage.CreatedOn = DateTime.UtcNow;

            await _systemMessageContainer.CreateItemAsync(systemMessage, new PartitionKey(systemMessage.UserID));
        }

        public async Task DeleteSystemMessages()
        {
            var query = _systemMessageContainer.GetItemQueryIterator<SystemMessage>(
                        new QueryDefinition("SELECT c.id, c.UserID FROM c"),
                        requestOptions: new QueryRequestOptions { MaxConcurrency = -1, MaxBufferedItemCount = 100 });

            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                foreach (var item in response)
                {
                    await _systemMessageContainer.DeleteItemAsync<SystemMessage>(
                        id: item.Id,
                        partitionKey: new PartitionKey(item.UserID)
                    );
                }
            }
        }

        public async Task<SystemMessage> GetSystemMessage()
        {
            var query = _systemMessageContainer.GetItemQueryIterator<SystemMessage>(
                new QueryDefinition("SELECT * FROM c ORDER BY c.CreatedOn ASC"),
                requestOptions: new QueryRequestOptions { MaxItemCount = 1 });

            if (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                return response.FirstOrDefault();
            }

            return null;
        }
    }
}
