using FEB.Service.Concrete;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;

namespace FEBAgent.Controllers
{
    [Route("api/v1/agents")]
    [ApiController]
    public class AgentController : ControllerBase
    {
        private OpenAIService service;

        public AgentController(OpenAIService service)
        {
            this.service = service;
        }

        [HttpPost("ask")]
        public async Task Ask([FromBody] QuestionRequest req)
        {
            // Set the response content type to text/event-stream (for server-sent events)
            Response.ContentType = "text/event-stream";

            // Make sure to keep the connection open by disabling response buffering
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");

            await foreach (string part in service.Ask(new FEB.Service.Dto.UserMessage()
            {
                Question = req.question,
                SessionKey = req.sessionKey
            }))
            {
                // Write each streamed part as it arrives
              
                await Response.WriteAsync($"data: {part}\n\n");
                await Response.Body.FlushAsync(); // Ensure the data is sent immediately
            }
        }



        public struct QuestionRequest
        {
            public string question { get; set; }
            public string sessionKey { get; set; }
        }
    }
}
