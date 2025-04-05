using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEBAgent.Domain
{
    public class Document:Entity
    {
        public string DocumentName { get; set; } = string.Empty; 
        public string UserID { get; set; } = string.Empty;       
        public string ParentDocumentId { get; set; }
        [JsonProperty(PropertyName = "/documents")]
        public string PartitionKey { get; set; } = "documents";
        public string Url { get; set; }
        public DateTime CreatedOn { get; set; }
        public int ChunkIndex { get; set; }                      
        public string Content { get; set; } = string.Empty;      
        public float[] Vector { get; set; } = Array.Empty<float>();

    }
}
