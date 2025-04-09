using FEB.Infrastructure.Configuration;
using FEB.Infrastructure.Dto;
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
                .Get<CosmosDbSettings>() ?? throw new Exception("CosmosDB configuration Required");
            _client = client;
            _container = _client.GetContainer(cosmosConfig.DatabaseName, "documents");
        }

        public async Task<List<Document>> GetDocuments()
        {
            var query = _container.GetItemLinqQueryable<Document>(true).ToFeedIterator();


            List<Document> results = new();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                results.AddRange(response);
            }

            return results;

        }

        public async Task AddDocument(Document document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            document.CreatedOn = DateTime.UtcNow;
            await _container.CreateItemAsync<Document>(document);
        }

        public void DeleteDocument(Document? document)
        {
            if (document == null) return;
            //_dbContext.Documents.Remove(document);
        }

        public async Task DeleteDocument(string documentID)
        {
            // Fetch the document by its ID to retrieve the UserID (partition key)
            var document = _container.GetItemLinqQueryable<Document>(true)
                                     .Where(d => d.Id == documentID)
                                     .AsEnumerable()
                                     .FirstOrDefault() ?? throw new Exception("Document not found");

            // Now use the UserID as the partition key
            var partitionKey = new Microsoft.Azure.Cosmos.PartitionKey(document.UserID);

            // Delete the document using the document ID and UserID as the partition key
            await _container.DeleteItemAsync<Document>(documentID, partitionKey);
        }

        public async Task<List<RelatedDocument>> GetRelatedDocuments(float[] questionVector, int knn)
        {
            var docs = await GetDocumentChunks();

            var relatedDocs = docs
                .Select(d => new RelatedDocument
                {
                    DocumentChunk = d,
                    Similarity = CosineSimilarity(questionVector, d.Vector)
                })
                .OrderByDescending(x => x.Similarity)
                .Take(knn)
                .ToList();
            return relatedDocs;
        }

        private float CosineSimilarity(float[] v1, float[] v2)
        {
            float dot = 0f, mag1 = 0f, mag2 = 0f;
            for (int i = 0; i < v1.Length; i++)
            {
                dot += v1[i] * v2[i];
                mag1 += v1[i] * v1[i];
                mag2 += v2[i] * v2[i];
            }
            return dot / (float)(Math.Sqrt(mag1) * Math.Sqrt(mag2) + 1e-8);
        }

        public async Task<List<DocumentChunk>> GetDocumentChunks()
        {
            var documents = await GetDocuments();

            return [.. documents.SelectMany(x => x.DocumentChunks)];
        }
    }
}
