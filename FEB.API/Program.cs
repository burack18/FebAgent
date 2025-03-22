using FEB.Infrastructure;
using FEB.Infrastructure.Repositories.Abstract;
using FEB.Infrastructure.Repositories.Concrete;
using FEB.Service.Abstract;
using FEB.Service.Concrete;
using FEB.Service.DocumentStorage;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
string api_key = builder.Configuration
    .GetSection("AppSettings")
    .GetValue<string>("ApiKey") ?? throw new Exception("ApiKey Required");


builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenAIChatCompletion(
    modelId: "gpt-4o-mini",
    apiKey: api_key
);
#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
builder.Services.AddOpenAITextEmbeddingGeneration(
    modelId: "text-embedding-ada-002",          // Name of the embedding model, e.g. "text-embedding-ada-002".
    apiKey: api_key
);
#pragma warning restore SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

builder.Services.AddSingleton<OpenAIService>();
builder.Services.AddSingleton<FebAgentContext>();

builder.Services.AddSingleton<IDocumentService, FileSystemStorage>();
builder.Services.AddSingleton<IDocumentRepository,DocumentRepository>();
builder.Services.AddSingleton<IChatMessageRepository, ChatMessageRepository>();
builder.Services.AddSingleton<IChatMessageService, ChatMessageService>();

builder.Services.AddSingleton<IConfigurationManager>(builder.Configuration); 

builder.Services.AddTransient((serviceProvider) =>
{   
    var kernel = new Kernel(serviceProvider);
    return kernel;
});

builder.Services.AddTransient((serviceProvider) => {

    return new Kernel(serviceProvider);
});
builder.Services.AddSwaggerGen();


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
