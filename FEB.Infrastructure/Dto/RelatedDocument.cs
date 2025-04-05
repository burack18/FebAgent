using FEBAgent.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEB.Infrastructure.Dto
{
    public class RelatedDocument
    {
        public Document Document{ get; set; }
        public float Similarity { get; set; }
    }
}
