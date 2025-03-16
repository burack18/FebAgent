using FEB.Infrastructure;
using FEB.Service.Abstract;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEB.Service
{
    public class DocumentStorageDecorator : IDocumentStorage
    {
        protected IDocumentRepository DocumentRepository;
        protected ILogger<DocumentStorageDecorator> logger;
        public DocumentStorageDecorator(IDocumentRepository documentRepository, ILogger<DocumentStorageDecorator> logger)
        {
            this.DocumentRepository = documentRepository;
            this.logger = logger;
        }
        public async virtual Task SaveDocuments(string userID, List<IFormFile> formFiles)
        {
            logger.LogInformation($"User with UserID:{userID} is saving files.");        
        }
    }
}
