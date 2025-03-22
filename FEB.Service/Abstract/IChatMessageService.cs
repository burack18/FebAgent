using FEBAgent.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEB.Service.Abstract
{
    public interface IChatMessageService
    {
        Task<List<ChatMessage>> GetChatMessages();
        Task<List<ChatMessage>> GetChatMessages(string sessionKey);
        void AddChatMessage(ChatMessage chatMessage);
        void DeleteChatMessage(ChatMessage chatMessage);
        void DeleteChatMessage(string chatMessageID);
        Task<bool> IsSessionExpired(string sessionKey);

    }
}
