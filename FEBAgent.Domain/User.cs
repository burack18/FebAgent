using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEBAgent.Domain
{
    public  class User:Entity
    {
        public string UserID { get; set; }
        public string FirstName { get; set; }=string.Empty;
        public string LastName { get; set; }= string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "/UserID")]
        public string PartitionKey => this.UserID;
    }
}
