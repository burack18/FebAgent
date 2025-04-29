using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEB.Infrastructure
{
    public static class Extensions
    {
        public static ChatHistory SanitizeForGemini(this ChatHistory history)
        {
            var messages = history
                .Where(m => m.Role == AuthorRole.User)
                .Select(m => new ChatMessageContent(m.Role, m.Content))
                .ToList();
            return new ChatHistory(messages);
        }

    }
}
