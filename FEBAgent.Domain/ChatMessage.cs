using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEBAgent.Domain
{
    public class ChatMessage : Entity
    {

        public string UserID { get; set; } = string.Empty;
        public string SessionKey { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public DateTime TimestampCreated { get; set; }

    }
}
