﻿using FEBAgent.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEB.Infrastructure
{
    public interface IDocumentRepository:IDisposable
    {
        Task<List<Document>> GetDocuments();
        void AddDocument(Document document);
        void DeleteDocument(Document document);
        void DeleteDocument(string documentID);
    }
}
