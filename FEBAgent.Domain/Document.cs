﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEBAgent.Domain
{
    public class Document:Entity
    {
        public string DocumentName { get; set; }=string.Empty;
        public string UserID { get; set; } = string.Empty;
    }
}
