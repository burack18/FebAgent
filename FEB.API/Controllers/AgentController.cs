using FEBAgent.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;

namespace FEBAgent.Controllers
{
    [Route("api/v1/agent")]
    [ApiController]
    public class AgentController : ControllerBase
    {
        private OpenAIService service;

        public AgentController(OpenAIService service)
        {
            this.service = service;
        }

        [HttpPost("ask")]
        public async Task<ChatMessageContent> Ask([FromBody] QuestionRequest req)
        {
            return await service.Ask(req.question);
        }

        public struct QuestionRequest
        {
            public string question { get; set; }
        }
    }
}
