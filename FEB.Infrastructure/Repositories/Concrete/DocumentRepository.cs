using FEB.Infrastructure.Configuration;
using FEB.Infrastructure.Repositories.Abstract;
using FEBAgent.Domain;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Cosmos.Serialization.HybridRow.Schemas;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;

namespace FEB.Infrastructure.Repositories.Concrete
{
    public class DocumentRepository : IDocumentRepository
    {
        private CosmosClient _client;
        private Container _container;

        public DocumentRepository(CosmosClient client, IConfiguration configuration)
        {
            var cosmosConfig = configuration
                .GetSection("CosmosDb")
                .Get<CosmosDbSettings>()??throw new Exception("CosmosDB configuration Required");
            _client = client;
            _container = _client.GetContainer(cosmosConfig.DatabaseName,"");
        }

        public async Task<List<Document>> GetDocuments()
        {
            var query = _container.GetItemLinqQueryable<Document>(true)
                                 .Where(d => d.PartitionKey == "document")
                                 .ToFeedIterator();
                                 

            List<Document> results = new();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                results.AddRange(response);
            }

            return results;
          
        }

        public void AddDocument(Document document)
        {
            //_dbContext.Documents.Add(document);
        }

        public void DeleteDocument(Document? document)
        {
            if (document == null) return;
            //_dbContext.Documents.Remove(document);
        }

        public void DeleteDocument(string documentID)
        {
            //var document = _dbContext.Documents.FirstOrDefault(x => x.Id == documentID);
            //DeleteDocument(document);
        }
    }
}
