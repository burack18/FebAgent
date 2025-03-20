using FEB.Infrastructure.Repositories.Abstract;
using FEBAgent.Domain;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEB.Infrastructure.Repositories.Concrete
{
    public class ChatMessageRepository : IChatMessageRepository
    {
        private FebAgentContext _dbContext;
        private IConfigurationManager _configurationManager;
        public ChatMessageRepository(FebAgentContext agentContext, IConfigurationManager configurationManager)
        {
            _dbContext = agentContext;
            _configurationManager = configurationManager;
        }
        public void AddChatMessage(ChatMessage chatMessage)
        {
            _dbContext.ChatMessages.Add(chatMessage);
        }

        public void DeleteChatMessage(ChatMessage chatMessage)
        {
            _dbContext.ChatMessages.Remove(chatMessage);
        }

        public void DeleteChatMessage(string chatMessageID)
        {
            var message = _dbContext.ChatMessages.FirstOrDefault(x => x.Id == chatMessageID);
            if (message == null) return;
            _dbContext.ChatMessages.Remove(message);
        }

        public async Task<List<ChatMessage>> GetChatMessages()
        {
            return _dbContext.ChatMessages.ToList();
        }

        public Task<List<ChatMessage>> GetChatMessages(string sessionKey)
        {
            return Task.FromResult(_dbContext.ChatMessages.Where(x => x.SessionKey == sessionKey).ToList());
        }

        public Task<bool> IsSessionExpired(string sessionKey)
        {
            var lastMessage = _dbContext.ChatMessages.Where(x => x.SessionKey == sessionKey).OrderByDescending(x => x.TimestampCreated).FirstOrDefault();
            var timeLimit = _configurationManager.GetSection("ChatHistory")["SessionTimeLimit"] ?? throw new Exception("Session Time Limit Required");
            var maxMessages = _configurationManager.GetSection("ChatHistory")["MaxMessages"] ?? throw new Exception("Message Count Limit Required");

            if (lastMessage == null) return Task.FromResult(true);

            if (DateTime.Now < lastMessage.TimestampCreated.AddMinutes(Convert.ToInt32(timeLimit))) return Task.FromResult(true);

            if (_dbContext.ChatMessages.Count >= Convert.ToInt32(maxMessages))return Task.FromResult(true);

            return Task.FromResult(false);
        }
    }
}
