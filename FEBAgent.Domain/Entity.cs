using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FEBAgent.Domain
{
    public class Entity
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        public DateTime CreatedOn { get; set; }

    }
}
