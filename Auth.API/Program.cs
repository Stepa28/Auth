using Auth.API.Extensions;
using Auth.API.Infrastructure;
using Auth.BusinessLayer.Services;
using Marvelous.Contracts.Enums;

var builder = WebApplication.CreateBuilder(args);
const string _logDirectoryVariableName = "LOG_DIRECTORY";
const string _secretKeyVariableName = "SUPER_SECRET_KYE";
const string _configUrlVariableName = "CONFIGS_SERVICE_URL";
var logDirectory = builder.Configuration.GetValue<string>(_logDirectoryVariableName);
var secretKey = builder.Configuration.GetValue<string>(_secretKeyVariableName);
var configUrl = builder.Configuration.GetValue<string>(_configUrlVariableName);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.RegisterSwaggerGen();

var config = new ConfigurationBuilder()
             .SetBasePath(logDirectory)
             .AddXmlFile("NLog.config", true, true)
             .Build();

builder.Services.RegisterDi();
builder.Services.RegisterLogger(config);
builder.Services.AddMemoryCache();
builder.Services.AddMassTransit();
builder.Services.AddCustomAuth(secretKey);
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddFluentValidation();

builder.Services.AddHostedService<HostedService>();

var app = builder.Build();

app.Configuration["secretKey"] = secretKey;
app.Configuration[$"{Microservice.MarvelousConfigs}Url"] = configUrl;

app.UseSwagger();
app.UseSwaggerUI();
app.UseMiddleware<ErrorExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();