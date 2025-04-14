using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FEB.Infrastructure.Repositories.Abstract;
using FEB.Infrastructure.Repositories.Concrete;
using FEB.Service.Abstract;
using FEB.Service.Concrete;
using FEBAgent.Domain;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
            //var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            //var documents = new List<Document>();
            return await this.DocumentRepository.GetDocuments();
            //await foreach (BlobItem blobItem in containerClient.GetBlobsAsync(traits: BlobTraits.Metadata))
            //{

            //    var blobClient = containerClient.GetBlobClient(blobItem.Name);

            //    // Extract UserID from blob name (assuming format: "userID/filename")
            //    var parts = blobItem.Name.Split('/');
            //    string userID = parts.Length > 1 ? parts[0] : "unknown";

            //    blobItem.Metadata.TryGetValue("filename", out string filename);
            //    blobItem.Metadata.TryGetValue("fileID", out string fileID);


            //    var document = new Document
            //    {
            //        Id = fileID,
            //        DocumentName = filename ?? "",
            //        UserID = userID,  // Extracted from blob path
            //        Url = blobClient.Uri.ToString(), // Public URL
            //        CreatedOn = blobItem.Properties.CreatedOn?.DateTime ?? DateTime.UtcNow,
            //    };

            //    documents.Add(document);
            //}

            //return documents;
        }

        public async Task SaveDocument(string userID, IFormFile file)
        {
            await this.SaveDocuments(userID, new List<IFormFile> { file });
        }

        public override async Task SaveDocuments(string userID, List<IFormFile> formFiles)
        {
            logger.LogInformation($"User with UserID:{userID} is saving files to Azure Blob Storage.");




            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);


            foreach (var file in formFiles)
            {

                // Process the document content with DocX
                using var stream = file.OpenReadStream();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                List<string> chunks=new List<string>();
                if (file.ContentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
                {
                    using var doc = DocX.Load(memoryStream);
                    string documentText = doc.Text;
                    chunks = ChunkByWords(documentText, 500);
                }
                else if (file.ContentType == "application/pdf")
                {
                    string pdfText = "";

                    using (var pdfReader = new PdfReader(memoryStream))
                    using (var pdfDoc = new PdfDocument(pdfReader))
                    {
                        for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
                        {
                            var strategy = new SimpleTextExtractionStrategy();
                            string textFromPage = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i), strategy);
                            pdfText += textFromPage + Environment.NewLine;
                        }
                    }

                    // Now chunk like before
                    chunks = ChunkByWords(pdfText, 500);
                }

                var vectors = await _openAIservice.Embed(chunks);

                var document = new Document()
                {
                    Id = Guid.NewGuid().ToString(),
                    DocumentName = file.Name,
                    UserID = userID,
                    Url = null, // set this if you have it
                    CreatedOn = DateTime.UtcNow,
                };

                for (int i = 0; i < chunks.Count; i++)
                {
                    string id = Guid.NewGuid().ToString();
                    var documentChunk = new DocumentChunk
                    {
                        Id = id,
                        DocumentChunkID = id,
                        Content = chunks[i],
                        CreatedOn = DateTime.UtcNow,
                        Vector = vectors[i].ToArray(),
                    };
                    document.DocumentChunks.Add(documentChunk);
                }

                await _documentRepository.AddDocument(document); // use the AddDocumentAsync method you built

                stream.Position = 0;

                var blobName = $"{userID}/{Guid.NewGuid()}_{file.FileName}";
                var blobClient = containerClient.GetBlobClient(blobName);
                IDictionary<string, string> metaData = new Dictionary<string, string>();
                metaData.Add(new KeyValuePair<string, string>("filename", file.FileName));
                metaData.Add(new KeyValuePair<string, string>("fileID", document.Id));

                var options = new BlobUploadOptions()
                {
                    HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType },
                    Metadata = metaData
                };
                await blobClient.UploadAsync(stream, options);


                logger.LogInformation($"File {file.FileName} uploaded successfully as {blobName}.");
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

            if (currentChunk.Count != 0)
            {
                chunks.Add(string.Join(" ", currentChunk));
            }

            return chunks;
        }

        public async Task DeleteDocumentByDocumentID(string documentID)
        {
            await _documentRepository.DeleteDocument(documentID);

            return;
        }
    }
}
