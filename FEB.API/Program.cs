using Azure.Storage;
using Azure.Storage.Blobs;
using FEB.Infrastructure;
using FEB.Infrastructure.Configuration;
using FEB.Service;
using FEB.Service.Abstract;
using FEB.Service.Concrete;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddScoped<IUserService, InMemoryUserService>(); // Use AddScoped or AddTransient as appropriate
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();


// Add authorization services (needed for [Authorize] attribute)
builder.Services.AddAuthorization();
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key not configured"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = builder.Environment.IsProduction(); // Enforce HTTPS in production
    options.SaveToken = true; // Save the token in the HttpContext
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true, // Validate the server that generates the token.
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true, // Validate the recipient of the token is authorized to receive it.
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true, // Check if the token is expired
        ClockSkew = TimeSpan.Zero // Remove default 5-minute clock skew
    };
    // You can add event handlers here if needed (e.g., OnAuthenticationFailed, OnTokenValidated)
});

builder.Services.AddSingleton<OpenAIService>();
builder.Services.AddSingleton<FebAgentContext>();

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
});



builder.Services.AddSingleton<IConfigurationManager>(builder.Configuration); 
builder.Services.AddInfrastucture(builder.Configuration);
builder.Services.AddService(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});




builder.Services.AddSwaggerGen(options => // Configure Swagger to use Bearer auth
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Please enter JWT with Bearer into field",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http, // Use Http for Bearer
        BearerFormat = "JWT",
        Scheme = "Bearer" // Use "Bearer" scheme
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement {
    {
        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Reference = new Microsoft.OpenApi.Models.OpenApiReference
            {
                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        Array.Empty<string>()
    }});
});



var app = builder.Build();
app.UseCors("AllowSpecificOrigin");

app.UseSwagger();
app.UseSwaggerUI();




//app.UseHttpsRedirection();

// Add Authentication middleware BEFORE Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
