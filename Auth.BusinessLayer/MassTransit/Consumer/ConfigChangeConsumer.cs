using Marvelous.Contracts.Configurations;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Auth.BusinessLayer.Consumer;

public class ConfigChangeConsumer : IConsumer<AuthCfg>
{
    private readonly IConfiguration _config;
    private readonly ILogger<ConfigChangeConsumer> _logger;

    public ConfigChangeConsumer(ILogger<ConfigChangeConsumer> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    public Task Consume(ConsumeContext<AuthCfg> context)
    {
        _logger.LogInformation($"Configuration {context.Message.Key} change value {_config[context.Message.Key]} to {context.Message.Value}");
        _config[context.Message.Key] = context.Message.Value;
        return Task.CompletedTask;
    }
}