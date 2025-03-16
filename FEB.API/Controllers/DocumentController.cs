using FEB.Service.Abstract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FEB.API.Controllers
{
    [Route("api/v1/documents")]
    [ApiController]
    public class DocumentController : ControllerBase
    {

        public IDocumentService DocumentService;

        public DocumentController(IDocumentService documentService)
        {
            DocumentService = documentService;
        }

        [HttpPost("loadDocuments")]
        public async Task LoadDocument()
        {
            var files = Request.Form.Files;
            foreach (var f in files)
            {
                await this.DocumentService.SaveDocument("U016061",f);
                Console.WriteLine(f.Name);
            }
            
        }
    }
}
