using Auth.API.Extensions;
using Auth.API.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var _logDirectoryVariableName = "LOG_DIRECTORY";
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
builder.Services.AddMassTransit();
builder.Services.AddMemoryCache();

var app = builder.Build();

//запустить инициализацию кеша

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.UseAuthorization();
app.UseMiddleware<ErrorExceptionMiddleware>();
app.MapControllers();
app.Run();