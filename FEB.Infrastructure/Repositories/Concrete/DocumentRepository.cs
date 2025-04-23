using FEB.Infrastructure.Configuration;
using FEB.Infrastructure.Dto;
using FEB.Infrastructure.Repositories.Abstract;
using FEBAgent.Domain;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Configuration;

namespace FEB.Infrastructure.Repositories.Concrete
{
    public class DocumentRepository : IDocumentRepository
    {
        private CosmosClient _client;
        private Container _docContainer;
        private Container _chunkContainer;

        public DocumentRepository(CosmosClient client, IConfiguration configuration)
        {
            var cosmosConfig = configuration
                .GetSection("CosmosDb")
                .Get<CosmosDbSettings>() ?? throw new Exception("CosmosDB configuration Required");
            _client = client;
            _docContainer = _client.GetContainer(cosmosConfig.DatabaseName, "documents");
            _chunkContainer = _client.GetContainer(cosmosConfig.DatabaseName, "chunks");
        }

        public async Task<List<Document>> GetDocuments()
        {
            var query = _docContainer.GetItemLinqQueryable<Document>(true).ToFeedIterator();


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
            await _docContainer.CreateItemAsync<Document>(document);
        }

        public void DeleteDocument(Document? document)
        {
            if (document == null) return;
            //_dbContext.Documents.Remove(document);
        }

        public async Task DeleteDocument(string documentID)
        {
            // Fetch the document by its ID to retrieve the UserID (partition key)
            var document = _docContainer.GetItemLinqQueryable<Document>(true)
                                     .Where(d => d.Id == documentID)
                                     .AsEnumerable()
                                     .FirstOrDefault() ?? throw new Exception("Document not found");

            // Now use the UserID as the partition key
            var partitionKey = new Microsoft.Azure.Cosmos.PartitionKey(document.UserID);

            // Delete the document using the document ID and UserID as the partition key
            await _docContainer.DeleteItemAsync<Document>(documentID, partitionKey);
            await DeleteChunksByDocumentID(documentID);
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
            foreach (var d in relatedDocs)
            {
                Console.WriteLine($"S=>{d.Similarity} id=>{d.DocumentChunk.Id}");
            }
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
            var query = _chunkContainer
                 .GetItemLinqQueryable<DocumentChunk>(allowSynchronousQueryExecution: false)
                 .ToFeedIterator();

            var results = new List<DocumentChunk>();

            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                results.AddRange(response);
            }

            return results;
        }

        public async Task<List<DocumentChunk>> GetDocumentChunks(string docID)
        {
            var query = _chunkContainer
                 .GetItemLinqQueryable<DocumentChunk>(allowSynchronousQueryExecution: false)
                 .Where(x => x.DocumentID == docID)
                 .ToFeedIterator();

            var results = new List<DocumentChunk>();

            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                results.AddRange(response);
            }

            return results;
        }

        public async Task AddChunks(List<DocumentChunk> docChunks)
        {
            if (docChunks == null) throw new ArgumentNullException(nameof(docChunks));

            var tasks = docChunks.Select(chunk =>
                _chunkContainer.CreateItemAsync<DocumentChunk>(chunk));

            await Task.WhenAll(tasks);
        }

        public async Task DeleteChunksByDocumentID(string docID)
        {
            var chunks = await GetDocumentChunks(docID);
            var partitionKey = new Microsoft.Azure.Cosmos.PartitionKey(docID);
            foreach (var chunk in chunks)
            {
                await _chunkContainer.DeleteItemAsync<DocumentChunk>(chunk.Id, partitionKey);
            }
        }
    }
}
