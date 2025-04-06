using Azure.Storage;
using Azure.Storage.Blobs;
using FEB.Infrastructure.Repositories.Abstract;
using FEB.Infrastructure.Repositories.Concrete;
using FEB.Service.Abstract;
using FEB.Service.Concrete;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEB.Service
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddService(this IServiceCollection services, IConfiguration config)
        {

            var blobKey = config
            .GetSection("BlobStorage").GetValue<string>("Key");
            var conn = config
                        .GetSection("BlobStorage").GetValue<string>("ConnectionString");
            var blobUrl = $"https://febagent.blob.core.windows.net";
            var credentials = new StorageSharedKeyCredential("febagent", blobKey);

            services.AddSingleton<IDocumentService, DocumentStorage.DocumentStorage>();
            services.AddSingleton<IDocumentRepository, DocumentRepository>();
            services.AddSingleton<IChatMessageRepository, ChatMessageRepository>();
            services.AddSingleton<IChatMessageService, ChatMessageService>();
            
            
            services.AddSingleton(new BlobServiceClient(conn));
            return services;
        }
    }
}
