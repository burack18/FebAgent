using FEB.Infrastructure.Dto;
using FEBAgent.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEB.Infrastructure.Repositories.Abstract
{
    public interface IDocumentRepository
    {
        Task<List<Document>> GetDocuments();
        Task AddDocument(Document document);
        void DeleteDocument(Document document);
        void DeleteDocument(string documentID);
        Task<List<RelatedDocument>> GetRelatedDocuments(float[] questionVector, int knn);
    }
}
