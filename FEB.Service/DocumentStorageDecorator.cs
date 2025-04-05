using FEB.Infrastructure.Repositories.Abstract;
using FEB.Service.Abstract;
using FEBAgent.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEB.Service
{
    public abstract class DocumentStorageDecorator : IDocumentStorage
    {
        protected IDocumentRepository DocumentRepository;
        protected ILogger<DocumentStorageDecorator> logger;
        public DocumentStorageDecorator(IDocumentRepository documentRepository, ILogger<DocumentStorageDecorator> logger)
        {
            this.DocumentRepository = documentRepository;
            this.logger = logger;
        }

        public abstract Task<List<Document>> GetDocuments();
       

        public async virtual Task SaveDocuments(string userID, List<IFormFile> formFiles)
        {
            logger.LogInformation($"User with UserID:{userID} is saving files.");        
        }
    }
}
