using Auth.API.Extensions;
using Auth.API.Infrastructure;
using Auth.BusinessLayer.Services;
using Marvelous.Contracts.Enums;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);
const string _logDirectoryVariableName = "LOG_DIRECTORY";
var logDirectory = builder.Configuration.GetValue<string>(_logDirectoryVariableName);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.RegisterSwaggerGen();

var config = new ConfigurationBuilder()
             .SetBasePath(logDirectory)
             .AddXmlFile("NLog.config", true, true)
             .Build();

builder.Services.RegisterServices();
builder.Services.RegisterLogger(config);
builder.Services.AddMemoryCache();
builder.Services.AddMassTransit();
builder.Services.AddCustomAuth();
builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();

//запуск инициализации кеша лидов
app.InitializationLeads();

//запуск инициализации моделей микросервисов
app.Services.GetRequiredService<IMemoryCache>().GetOrCreate(nameof(Microservice),
    new InitializeMicroserviceModels(app.Services.GetRequiredService<ILogger<InitializeMicroserviceModels>>())
        .InitializeMicroservices);

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseMiddleware<ErrorExceptionMiddleware>();
app.MapControllers();
app.Run();