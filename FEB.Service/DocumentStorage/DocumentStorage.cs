using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FEB.Infrastructure.Repositories.Abstract;
using FEB.Infrastructure.Repositories.Concrete;
using FEB.Service.Abstract;
using FEB.Service.Concrete;
using FEBAgent.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Words.NET;

namespace FEB.Service.DocumentStorage
{
    public class DocumentStorage : DocumentStorageDecorator, IDocumentService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName = "documents";
        private readonly IDocumentRepository _documentRepository;
        private readonly OpenAIService _openAIservice;
        
        public DocumentStorage(IDocumentRepository documentRepository, 
            ILogger<DocumentStorage> logger, 
            BlobServiceClient blobServiceClient, 
            OpenAIService openAIservice)
            : base(documentRepository, logger)
        {
            _blobServiceClient = blobServiceClient;
            _openAIservice = openAIservice;
            _documentRepository = documentRepository;
        }

        public override async Task<List<Document>> GetDocuments()
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var documents = new List<Document>();

            await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
            {
                var blobClient = containerClient.GetBlobClient(blobItem.Name);

                // Extract UserID from blob name (assuming format: "userID/filename")
                var parts = blobItem.Name.Split('/');
                string userID = parts.Length > 1 ? parts[0] : "unknown";

                var document = new Document
                {
                    DocumentName = Path.GetFileName(blobItem.Name),
                    UserID = userID,  // Extracted from blob path
                    Url = blobClient.Uri.ToString(), // Public URL
                    CreatedOn = blobItem.Properties.CreatedOn?.DateTime ?? DateTime.UtcNow,
                    PartitionKey = "documents",
                    ParentDocumentId = null,  // Not available from blob storage
                    ChunkIndex = 0, // Default since blobs are whole files
                    Content = "", // We don't fetch content from blob in this method
                    Vector = Array.Empty<float>() // Not stored in blob metadata
                };

                documents.Add(document);
            }

            return documents;
        }

        public async Task SaveDocument(string userID, IFormFile file)
        {
            await this.SaveDocuments(userID, new List<IFormFile> { file });
        }

        public override async Task SaveDocuments(string userID, List<IFormFile> formFiles)
        {
            logger.LogInformation($"User with UserID:{userID} is saving files to Azure Blob Storage.");




            //var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);


            foreach (var file in formFiles)
            {
                // Process the document content with DocX
                using var stream = file.OpenReadStream();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                using var doc = DocX.Load(memoryStream);
                string documentText = doc.Text;
                List<string> chunks = ChunkByWords(documentText, 100);

                var vectors = await _openAIservice.Embed(chunks);

                for (int i = 0; i < chunks.Count; i++)
                {
                    var document = new Document
                    {
                        Id = Guid.NewGuid().ToString(),
                        DocumentName = file.Name,
                        UserID = userID,
                        ParentDocumentId = null, // set this if you have it
                        Content = chunks[i],
                        ChunkIndex = i,
                        CreatedOn = DateTime.UtcNow,
                        Vector = vectors[i].ToArray(), // this converts ReadOnlyMemory<float> to float[]
                    };

                    await _documentRepository.AddDocument(document); // use the AddDocumentAsync method you built
                }


                //var blobName = $"{userID}/{Guid.NewGuid()}_{file.FileName}";
                //var blobClient = containerClient.GetBlobClient(blobName);


                //await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });


                //logger.LogInformation($"File {file.FileName} uploaded successfully as {blobName}.");
            }
        }
        public static List<string> ChunkByWords(string text, int maxWords)
        {
            var words = text.Split(' ');
            var chunks = new List<string>();
            var currentChunk = new List<string>();

            foreach (var word in words)
            {
                currentChunk.Add(word);
                if (currentChunk.Count >= maxWords)
                {
                    chunks.Add(string.Join(" ", currentChunk));
                    currentChunk.Clear();
                }
            }

            if (currentChunk.Any())
            {
                chunks.Add(string.Join(" ", currentChunk));
            }

            return chunks;
        }
    }
}
