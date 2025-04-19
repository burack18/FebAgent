using FEB.Infrastructure.Repositories.Abstract;
using FEB.Service.Abstract;
using FEBAgent.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEB.Service.Concrete
{
    public class SystemMessageService : ISystemMessageService
    {
        private readonly ISystemMessageRepository _systemMessageRepository;
        public SystemMessageService(ISystemMessageRepository systemMessageRepository)
        {
            _systemMessageRepository = systemMessageRepository;
        }
        public async Task AddSystemMessage(SystemMessage systemMessage)
        {
            await DeleteSystemMessages();
            await _systemMessageRepository.AddSystemMessage(systemMessage);
        }

        public async Task DeleteSystemMessages()
        {
            await _systemMessageRepository.DeleteSystemMessages();
        }

        public async Task<SystemMessage> GetSystemMessage()
        {
            var systemMessage = await _systemMessageRepository.GetSystemMessage();
            return systemMessage;
        }
    }
}
