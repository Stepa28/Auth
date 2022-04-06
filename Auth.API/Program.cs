using Auth.API.Extensions;
using Auth.API.Infrastructure;
using Auth.BusinessLayer.Services;
using Marvelous.Contracts.Enums;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);
const string _logDirectoryVariableName = "LOG_DIRECTORY";
const string _secretKeyVariableName = "SUPER_SECRET_KYE";
const string _addressConfigVariableName = "ADDRESS_CONFIGS_FOR_AUTH";
var logDirectory = builder.Configuration.GetValue<string>(_logDirectoryVariableName);
var secretKey = builder.Configuration.GetValue<string>(_secretKeyVariableName);
var addressConfig = builder.Configuration.GetValue<string>(_addressConfigVariableName);

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
builder.Services.AddCustomAuth(secretKey);
builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();

//запуск инициализации моделей микросервисов
app.Services.GetRequiredService<IMemoryCache>().Set(nameof(Microservice), new InitializeMicroserviceModels(app.Configuration).InitializeMicroservices());

app.UseSwagger();
app.UseSwaggerUI();
app.UseMiddleware<ErrorExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.RunAsync();

//запуск инициализации конфигурации
app.InitializationConfiguration(secretKey, addressConfig);

//запуск инициализации кеша лидов
app.InitializationLeads();
Console.ReadKey();