
using FEB.API.Dto;
using FEB.Service.Abstract;
using FEBAgent.Domain;
using Microsoft.AspNetCore.Mvc;

namespace FEB.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class SystemMessageController
    {
        private readonly ISystemMessageService systemMessageService;
        public SystemMessageController(ISystemMessageService systemMessageService)
        {
            this.systemMessageService = systemMessageService;
        }
        [HttpGet]
        public async Task<SystemMessage> GetSystemMessage()
        {
            var systemMessage = await systemMessageService.GetSystemMessage();
            return systemMessage;
        }
        [HttpPost]
        public async Task<IActionResult> AddSystemMessage([FromBody] SystemMessageDto systemMessageDto)
        {
            if (systemMessageDto == null)
            {
                return new BadRequestResult();
            }
            var systemMessage = new SystemMessage
            {
                CreatedOn = DateTime.Now,
                Message = systemMessageDto.Message,
                UserID = systemMessageDto.UserID
            };
            await systemMessageService.AddSystemMessage(systemMessage);
            return new OkResult();
        }
    }
}
