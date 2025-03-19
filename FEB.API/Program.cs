using FEB.Infrastructure;
using FEB.Infrastructure.Repositories.Abstract;
using FEB.Infrastructure.Repositories.Concrete;
using FEB.Service.Abstract;
using FEB.Service.Concrete;
using FEB.Service.DocumentStorage;
using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
string api_key = builder.Configuration.GetSection("AppSettings").GetValue<string>("ApiKey") ?? throw new Exception("ApiKey Required");


builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddOpenAIChatCompletion(
    modelId: "gpt-4o-mini",
    apiKey: api_key
);

builder.Services.AddSingleton<OpenAIService>();
builder.Services.AddSingleton<FebAgentContext>();

builder.Services.AddSingleton<IDocumentService, FileSystemStorage>();
builder.Services.AddSingleton<IDocumentRepository,DocumentRepository>();
builder.Services.AddSingleton<IChatMessageRepository, ChatMessageRepository>();
builder.Services.AddSingleton<IChatMessageService, ChatMessageService>();

builder.Services.AddSingleton<IConfigurationManager>(builder.Configuration); 

builder.Services.AddTransient((serviceProvider) => {
    var kernel = new Kernel(serviceProvider);
    return kernel;
});

builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
