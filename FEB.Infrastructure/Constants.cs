using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEB.Infrastructure.Concrete
{
    public static class Constants
    {
        public enum AgentService
        {
            OPENAI,
            GEMINI
        }
        public static string ToService(this AgentService service)
        {
            return service switch
            {
                AgentService.OPENAI => "openai-chat",
                AgentService.GEMINI => "gemini-chat",
                _ => throw new ArgumentOutOfRangeException(nameof(service), service, null)
            };
        }
    }
}
