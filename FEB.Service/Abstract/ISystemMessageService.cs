using FEBAgent.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEB.Service.Abstract
{
    public interface ISystemMessageService
    {
        Task<SystemMessage> GetSystemMessage();
        Task AddSystemMessage(SystemMessage systemMessage);
        Task DeleteSystemMessages();
    }
}
