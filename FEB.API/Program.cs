using FEB.Infrastructure;
using FEB.Infrastructure.Repositories.Abstract;
using FEB.Infrastructure.Repositories.Concrete;
using FEB.Service.Abstract;
using FEB.Service.Concrete;
using FEB.Service.DocumentStorage;


var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;




builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();



builder.Services.AddSingleton<OpenAIService>();
builder.Services.AddSingleton<FebAgentContext>();

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
});


builder.Services.AddSingleton<IDocumentService, FileSystemStorage>();
builder.Services.AddSingleton<IDocumentRepository,DocumentRepository>();
builder.Services.AddSingleton<IChatMessageRepository, ChatMessageRepository>();
builder.Services.AddSingleton<IChatMessageService, ChatMessageService>();

builder.Services.AddSingleton<IConfigurationManager>(builder.Configuration); 
builder.Services.AddInfrastucture(builder.Configuration);



builder.Services.AddSwaggerGen();



var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();




//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
