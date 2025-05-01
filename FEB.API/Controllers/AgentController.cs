using FEB.Infrastructure.Concrete;
using FEB.Service.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using System.Security.Claims;

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
            var userID = User.FindFirstValue("UserID");
            var response = await service.Ask(new FEB.Service.Dto.UserMessage()
            {
                Question = req.question,
                SessionKey = req.sessionKey,
                UserID=userID
            }, req.service);
            return response;
        }

        [Authorize]
        [HttpPost("clearhistory")]
        public async Task ClearChatHistory()
        {
            var userID = User.FindFirstValue("UserID");

            await service.ClearChatHistory(userID);
        }



        public struct QuestionRequest
        {
            public string question { get; set; }
            public string sessionKey { get; set; }

            public Constants.AgentService service { get; set; }
        }
    }
}
