using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEBAgent.Domain
{
    public class SystemMessage:Entity
    {
        public string Message { get; set; }
        public string UserID { get; set; } = string.Empty;
        [JsonProperty(PropertyName = "/UserID")]
        public string PartitionKey => this.UserID;
    }
}
