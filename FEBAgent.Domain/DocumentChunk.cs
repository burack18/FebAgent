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
        public string Content { get; set; } = string.Empty;
        public float[] Vector { get; set; } = Array.Empty<float>();
        public DateTime CreatedOn { get; set; }
    }
}
