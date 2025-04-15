using FEB.Service.Concrete;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize]
        [HttpPost("ask")]
        public async Task<string> Ask([FromBody] QuestionRequest req)
        {

            var response = await service.Ask(new FEB.Service.Dto.UserMessage()
            {
                Question = req.question,
                SessionKey = req.sessionKey
            });
            return response;
        }



        public struct QuestionRequest
        {
            public string question { get; set; }
            public string sessionKey { get; set; }
        }
    }
}
