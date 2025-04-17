using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEBAgent.Domain
{
    public class DocumentChunk:Entity
    {
        public string DocumentChunkID { get; set; }
        public string DocumentID { get; set; }
        [JsonProperty(PropertyName = "/DocumentID")]
        public string PartitionKey => this.DocumentID;
        public string Content { get; set; } = string.Empty;
        public float[] Vector { get; set; } = Array.Empty<float>();
        public DateTime CreatedOn { get; set; }
    }
}
