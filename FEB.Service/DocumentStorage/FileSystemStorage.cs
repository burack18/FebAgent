using FEB.Infrastructure;
using FEB.Service.Abstract;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEB.Service.DocumentStorage
{
    public class FileSystemStorage : DocumentStorageDecorator, IDocumentService
    {
        private IConfigurationManager ConfigurationManager;
        public FileSystemStorage(IDocumentRepository documentRepository, ILogger<DocumentStorageDecorator> logger, IConfigurationManager configurationManager) : base(documentRepository, logger)
        {
            this.ConfigurationManager = configurationManager;
        }

        public async Task SaveDocument(string userID, IFormFile file)
        {
            await this.SaveDocuments(userID, [file]);
        }

        public override async Task SaveDocuments(string userID, List<IFormFile> formFiles)
        {

            await base.SaveDocuments(userID, formFiles);
            string savePath = this.ConfigurationManager["StoragePath"] ?? throw new Exception("Save PATH is required");

            foreach (var file in formFiles)
            {
                Guid generatedName = Guid.NewGuid();
                var extension = Path.GetExtension(file.FileName).ToLower();
                string filePath = Path.Combine(savePath, generatedName.ToString()+extension);

                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

            }
        }
    }
}
