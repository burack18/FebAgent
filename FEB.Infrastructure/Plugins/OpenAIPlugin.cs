using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEB.Service.Plugins
{
    public class OpenAIPlugin
    {
        [KernelFunction]
        [Description("Returns the current date and time of the user.")]
        public DateTime GetCurrentTime()
        {
            return DateTime.Now;
        }
    }
}
