using Auth.API.Extensions;
using Auth.API.Infrastructure;
using Auth.BusinessLayer.Helpers;
using Auth.BusinessLayer.Services;
using AutoMapper;
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
builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();

//запуск инициализации кеша
new InitializationService(new RequestHelper(), app.Services.GetRequiredService<ILogger<InitializationService>>(), app.Services.GetRequiredService<IMapper>(), app.Services.GetRequiredService<IMemoryCache>()).InitializeMamoryCash();
GC.Collect();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseMiddleware<ErrorExceptionMiddleware>();
app.MapControllers();
app.Run();