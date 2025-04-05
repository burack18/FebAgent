using Azure.Storage;
using Azure.Storage.Blobs;
using FEB.Infrastructure;
using FEB.Infrastructure.Configuration;
using FEB.Service;
using FEB.Service.Concrete;


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



builder.Services.AddSingleton<IConfigurationManager>(builder.Configuration); 
builder.Services.AddInfrastucture(builder.Configuration);
builder.Services.AddService(builder.Configuration);




builder.Services.AddSwaggerGen();



var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();




//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
