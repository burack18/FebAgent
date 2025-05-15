using FEB.Infrastructure.Repositories.Abstract;
using FEB.Service.Abstract;
using FEBAgent.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FEB.API.Controllers
{
    [ApiController]
    [Route("api/v1/documents")]
    public class DocumentController : ControllerBase
    {

        private readonly IDocumentService _documentService;
        
        public DocumentController(IDocumentService documentService)
        {
            _documentService = documentService;
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
            var userID = User.FindFirstValue("UserID");

            foreach (var file in files)
            {
                if (file.ContentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                 || file.ContentType == "application/pdf")
                {
                    // Save the document to the service
                    await _documentService.SaveDocument(userID, file);

        
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
