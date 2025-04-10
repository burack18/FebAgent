using FEB.Infrastructure.Repositories.Abstract;
using FEB.Service.Abstract;
using FEB.Service.Dto;
using FEBAgent.Domain;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace FEB.Service.Concrete
{
    public class OpenAIService
    {
        private Kernel _kernel;
        private IChatMessageService _chatMessageService;
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        private ITextEmbeddingGenerationService _textEmbeddingGenerationService;
        private readonly IDocumentRepository _documentRepository;

        public OpenAIService(Kernel kernel, IChatMessageService chatMessageService, ITextEmbeddingGenerationService textEmbeddingGenerationService, IDocumentRepository documentRepository)
        {
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            _kernel = kernel;
            _chatMessageService = chatMessageService;
            _textEmbeddingGenerationService = textEmbeddingGenerationService;
            _documentRepository = documentRepository;
        }
        public async IAsyncEnumerable<string> Ask(UserMessage userMessage)
        {
            //var isSessionExpired = await _chatMessageService.IsSessionExpired(userMessage.SessionKey);

            //if (isSessionExpired) throw new Exception("Session expired");



            var message = new ChatMessage
            {
                Message = userMessage.Question,
                TimestampCreated = DateTime.Now,
                SessionKey = userMessage.SessionKey ?? Guid.NewGuid().ToString(),
            };

            _chatMessageService.AddChatMessage(message);
            var chatHistory = new ChatHistory();

            var questionVector = await Embed([userMessage.Question]);
            var relatedDocInfo = string.Empty;

            if (questionVector.Count > 0)
            {
                var relatedDocuments = await _documentRepository.GetRelatedDocuments(questionVector[0].ToArray(), 3);
                foreach (var d in relatedDocuments)
                {
                    relatedDocInfo += "\n" + d.DocumentChunk.Content;
                }
            }

            chatHistory.AddSystemMessage("""
        You are a helpful AI assistant. Only respond based on the given information. Do not use external knowledge or assumptions beyond the provided content.

        Follow these 4 steps to answer accurately:

        Read the user's question carefully. Identify the exact information being asked.

        Scan the provided document content for relevant sections that address the question.

        Extract only the necessary details from the document to form your answer.

        Respond clearly and concisely, using only information found in the document. Do not infer or assume anything beyond what is written.

        Related Document content is:
        """ + relatedDocInfo);

            chatHistory.AddUserMessage(userMessage.Question);

            var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

            // 🔁 Stream the response content
            await foreach (var content in chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory, kernel: _kernel))
            {
                if (content.Content != null)
                {
                    // Return the current chunk
                    yield return content.Content;

                }
            }
        }
   
        public async Task<IList<ReadOnlyMemory<float>>> Embed(List<string> chunks)
        {
            IList<ReadOnlyMemory<float>> embeddings =
                        await _textEmbeddingGenerationService.GenerateEmbeddingsAsync(chunks);
            return embeddings;
        }
    }
}
