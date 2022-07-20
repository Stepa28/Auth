using System.Globalization;
using Auth.API.Extensions;
using Auth.API.Infrastructure;
using Auth.BusinessLayer.Configuration;
using Auth.BusinessLayer.Services;
using Marvelous.Contracts.Enums;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);
const string logDirectoryVariableName = "LOG_DIRECTORY";
const string secretKeyVariableName = "SUPER_SECRET_KYE";
const string configUrlVariableName = "CONFIGS_SERVICE_URL";
var logDirectory = builder.Configuration.GetValue<string>(logDirectoryVariableName);
var secretKey = builder.Configuration.GetValue<string>(secretKeyVariableName);
var configUrl = builder.Configuration.GetValue<string>(configUrlVariableName);

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
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
builder.Services.AddFluentValidation();

builder.Services.AddHostedService<HostedService>();
builder.Services.AddLocalization();

var app = builder.Build();

app.Configuration["secretKey"] = secretKey;
app.Configuration[$"{Microservice.MarvelousConfigs}Url"] = configUrl;

var supportedCultures = new[]
{
    new CultureInfo("en"),
    new CultureInfo("ru")
};
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

app.UseSwagger();
app.UseSwaggerUI();
app.UseMiddleware<ErrorExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();