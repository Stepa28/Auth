using FluentValidation;
using Marvelous.Contracts.Configurations;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Auth.BusinessLayer.Consumer;

public class ConfigChangeConsumer : IConsumer<AuthCfg>
{
    private readonly IConfiguration _config;
    private readonly ILogger<ConfigChangeConsumer> _logger;
    private readonly IValidator<AuthCfg> _validator;

    public ConfigChangeConsumer(ILogger<ConfigChangeConsumer> logger, IConfiguration config, IValidator<AuthCfg> validator)
    {
        _logger = logger;
        _config = config;
        _validator = validator;
    }

    public Task Consume(ConsumeContext<AuthCfg> context)
    {
        _validator.ValidateAndThrow(context.Message);
        _logger.LogInformation($"Configuration {context.Message.Key} change value {_config[context.Message.Key]} to {context.Message.Value}");
        _config[context.Message.Key] = context.Message.Value;
        return Task.CompletedTask;
    }
}