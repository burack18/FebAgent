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
    public class ChatMessageService : IChatMessageService
    {
        private readonly IChatMessageRepository _repository;

        public ChatMessageService(IChatMessageRepository repository)
        {
            _repository = repository;
        }

        public void AddChatMessage(ChatMessage chatMessage)
        {
            _repository.AddChatMessage(chatMessage);
        }

        public void DeleteChatMessage(ChatMessage chatMessage)
        {
            _repository.DeleteChatMessage(chatMessage);
        }

        public void DeleteChatMessage(string chatMessageID)
        {
            _repository.DeleteChatMessage(chatMessageID);
        }

        public Task<List<ChatMessage>> GetChatMessages()
        {
            return _repository.GetChatMessages();
        }

        public Task<List<ChatMessage>> GetChatMessages(string sessionKey)
        {
            return _repository.GetChatMessages(sessionKey);
        }

        public Task<bool> IsSessionExpired(string sessionKey)
        {
            return _repository.IsSessionExpired(sessionKey);
        }
    }
}
