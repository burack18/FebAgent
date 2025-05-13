using FEB.Infrastructure.Concrete;
using FEB.Infrastructure.Dto;
using FEB.Service.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEB.Service.Abstract
{
    public interface IOpenAIService
    {
        Task<string> Ask(UserMessage userMessage, Constants.AgentService service);
        Task<string> AskSmart(UserMessage userMessage, Constants.AgentService service);
        Task<string> AskSmartStream(UserMessage userMessage, Constants.AgentService service, List<RelatedDocument> relatedDocumentsCollection);
        Task<List<RelatedDocument>> GetRelatedDocuments(List<string> enrichedQuestion);
        Task<IList<ReadOnlyMemory<float>>> Embed(List<string> chunks);
        Task<List<string>> EnrichQuestion(string question, Constants.AgentService service);
        Task ClearChatHistory(string userID);
    }
}
