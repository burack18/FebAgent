using FEB.Service.Abstract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xceed.Words.NET;

namespace FEB.API.Controllers
{
    [Route("api/v1/documents")]
    [ApiController]
    public class DocumentController : ControllerBase
    {

        private readonly IDocumentService _documentService;

        public DocumentController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

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

                    // Process the document content with DocX
                    using var stream = file.OpenReadStream();
                    using var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    using var doc = DocX.Load(memoryStream);
                    string documentText = doc.Text;
                    List<string> chunks = ChunkByWords(documentText,100);
                    Console.WriteLine($"Document Name: {file.FileName}");
                    Console.WriteLine($"Document Content: {documentText}");
                }
                else
                {
                    Console.WriteLine($"Unsupported file type: {file.ContentType}");
                }
            }
        }
        public static List<string> ChunkByWords(string text, int maxWords)
        {
            var words = text.Split(' ');
            var chunks = new List<string>();
            var currentChunk = new List<string>();

            foreach (var word in words)
            {
                currentChunk.Add(word);
                if (currentChunk.Count >= maxWords)
                {
                    chunks.Add(string.Join(" ", currentChunk));
                    currentChunk.Clear();
                }
            }

            if (currentChunk.Any())
            {
                chunks.Add(string.Join(" ", currentChunk));
            }

            return chunks;
        }

    }
}
