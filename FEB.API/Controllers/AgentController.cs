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
        public async Task<ChatMessageContent> Ask([FromBody] QuestionRequest req)
        {
            return await service.Ask(new FEB.Service.Dto.UserMessage()
            {
                Question = req.question,
                SessionKey = req.sessionKey
            });
        }



        public struct QuestionRequest
        {
            public string question { get; set; }
            public string sessionKey { get; set; }
        }
    }
}
