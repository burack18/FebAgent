using FEB.Infrastructure.Repositories.Abstract;
using FEBAgent.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEB.Infrastructure.Repositories.Concrete
{
    public class DocumentRepository : IDocumentRepository
    {
        private FebAgentContext _dbContext;
        public DocumentRepository(FebAgentContext agentContext)
        {
            _dbContext = agentContext;
        }
        public async Task<List<Document>> GetDocuments()
        {
            return await Task.FromResult(_dbContext.Documents);
        }

        public void AddDocument(Document document)
        {
            _dbContext.Documents.Add(document);
        }

        public void DeleteDocument(Document? document)
        {
            if (document == null) return;
            _dbContext.Documents.Remove(document);
        }

        public void DeleteDocument(string documentID)
        {
            var document = _dbContext.Documents.FirstOrDefault(x => x.Id == documentID);
            DeleteDocument(document);
        }
    }
}
