using FEB.Infrastructure.Repositories.Abstract;
using FEB.Service.Abstract;
using FEBAgent.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Xceed.Words.NET;

namespace FEB.API.Controllers
{
    [Route("api/v1/documents")]
    [ApiController]
    public class DocumentController : ControllerBase
    {

        private readonly IDocumentService _documentService;
        private readonly IDocumentRepository documentRepository;
        
        public DocumentController(IDocumentService documentService, IDocumentRepository documentRepository)
        {
            _documentService = documentService;
            this.documentRepository = documentRepository;
        }
        [Authorize]
        [HttpGet("")]
        public async Task<List<Document>> GetDocuments()
        {
            //return await this._documentService.GetDocuments();
            return await this._documentService.GetDocuments();
        }

        [Authorize]
        [HttpPost("loadDocuments")]
        public async Task LoadDocument()
        {
            var files = Request.Form.Files;

            foreach (var file in files)
            {
                if (file.ContentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
                {
                    // Save the document to the service
                    await _documentService.SaveDocument("U016061", file);

        
                }
                else
                {
                    Console.WriteLine($"Unsupported file type: {file.ContentType}");
                }
            }
        }
        [Authorize]
        [HttpDelete("{documentID}")]
        public async Task<IActionResult> DeleteDocument(string documentID)
        {
            await this._documentService.DeleteDocumentByDocumentID(documentID);
            return Ok();
        }

    }
}
