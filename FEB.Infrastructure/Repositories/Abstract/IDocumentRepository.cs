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
        Task<List<DocumentChunk>> GetDocumentChunks();
        Task AddDocument(Document document);
        void DeleteDocument(Document document);
        Task DeleteDocument(string documentID);
        Task<List<RelatedDocument>> GetRelatedDocuments(float[] questionVector, int knn);
    }
}
