using FEB.Infrastructure.Concrete;
using FEB.Service.Concrete;
using FEB.Service.Dto;
using iText.Commons.Bouncycastle.Cert.Ocsp;
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
                UserID = userID
            }, req.service);
            return response;
        }

        [Authorize]
        [HttpPost("ask-smart")]
        public async Task<string> AskSmartAgent([FromBody] QuestionRequest req)
        {
            var userID = User.FindFirstValue("UserID");
            var response = await service.AskSmart(new FEB.Service.Dto.UserMessage()
            {
                Question = req.question,
                SessionKey = req.sessionKey,
                UserID = userID
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

        [HttpPost("ask-smart-stream")]
        public async Task StreamText([FromBody] QuestionRequest req)
        {
            Response.ContentType = "text/plain";

            var userID = User.FindFirstValue("UserID");

            var enrichmentQuestions = await service.EnrichQuestion(req.question, req.service);

            if (enrichmentQuestions.Count > 0)
            {
                await Response.WriteAsync($"**Query Expansion Questions** \n");
                await Response.Body.FlushAsync(); // ensures data is sent immediately
                await Task.Delay(10);
            }



            int index = 0;
            foreach (var question in enrichmentQuestions)
            {
                index++;
                await Response.WriteAsync($"{index}. {question} \n");
                await Response.Body.FlushAsync();
                await Task.Delay(10);
            }
            if (enrichmentQuestions.Count > 0)
            {
                await Response.WriteAsync($" **Generating Answer...** \n");
                await Response.Body.FlushAsync();
                await Task.Delay(10);
            }

            var documents = await service.GetRelatedDocuments([.. enrichmentQuestions, req.question]);

            if (documents.Count > 0)
            {
                await Response.WriteAsync($"**Reading Related Documents...** \n");
                await Response.Body.FlushAsync();
                await Task.Delay(10);
            }

            foreach (var doc in documents)
            {
                await Response.WriteAsync($"{doc.DocumentChunk.Content} \n");
                await Response.Body.FlushAsync();
                await Task.Delay(1000);
            }


            var response = await service.AskSmartStream(new FEB.Service.Dto.UserMessage()
            {
                Question = req.question,
                SessionKey = req.sessionKey,
                UserID = userID
            }, req.service, documents);

            await Response.WriteAsync($"PREQUESTIONEND");
            await Response.Body.FlushAsync();
            await Task.Delay(10);

            for (int i = 0; i < response.Length; i++)
            {
                await Response.WriteAsync($"{response[i]}");
                await Response.Body.FlushAsync();
                await Task.Delay(10);
            }
        }

        public struct QuestionRequest
        {
            public string question { get; set; }
            public string sessionKey { get; set; }

            public Constants.AgentService service { get; set; }
        }
    }
}
