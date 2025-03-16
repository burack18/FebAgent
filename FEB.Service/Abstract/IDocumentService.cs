using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEB.Service.Abstract
{
    public interface IDocumentService
    {
        Task SaveDocuments(string userID,List<IFormFile> files);
        Task SaveDocument(string userID,IFormFile file);
    }
}
