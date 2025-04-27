using FEB.Infrastructure.Concrete;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FEB.Infrastructure.Concrete.Constants;

namespace FEB.Infrastructure
{
    public class PromptExecutionSettingsFactory
    {
        public static PromptExecutionSettings? CreatePromptSettings(Constants.AgentService service)
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            PromptExecutionSettings settings = service switch
            {
                AgentService.OPENAI => new()
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                },
                AgentService.GEMINI => null,
                _ => throw new ArgumentOutOfRangeException(nameof(service), service, null)
            };
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.


            return settings;
        }
    }
}
