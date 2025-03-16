using FEBAgent.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEB.Infrastructure
{
    public class FebAgentContext : IDocumentRepository
    {

        private bool disposedValue;

        public List<Document> Documents { get; set; } = [];








        //Ignore
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public async Task<List<Document>> GetDocuments()
        {
            return await Task.FromResult(this.Documents);
        }

        public void AddDocument(Document document)
        {
            this.Documents.Add(document);
        }

        public void DeleteDocument(Document? document)
        {
            if (document == null) return;
            this.Documents.Remove(document);
        }

        public void DeleteDocument(string documentID)
        {
            var document = this.Documents.FirstOrDefault(x => x.Id == documentID);
            this.DeleteDocument(document);
        }
    }
}
