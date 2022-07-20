using Auth.Resources;
using FluentValidation;
using Marvelous.Contracts.Configurations;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace Auth.BusinessLayer.Consumer;

public class ConfigChangeConsumer : IConsumer<AuthCfg>
{
    private readonly IConfiguration _config;
    private readonly ILogger<ConfigChangeConsumer> _logger;
    private readonly IValidator<AuthCfg> _validator;
    private readonly IStringLocalizer<ExceptionAndLogMessages> _localizer;

    public ConfigChangeConsumer(ILogger<ConfigChangeConsumer> logger, IConfiguration config, IValidator<AuthCfg> validator, IStringLocalizer<ExceptionAndLogMessages> localizer)
    {
        _logger = logger;
        _config = config;
        _validator = validator;
        _localizer = localizer;
    }

    public Task Consume(ConsumeContext<AuthCfg> context)
    {
        _validator.ValidateAndThrow(context.Message);
        _logger.LogInformation(_localizer["ConfigurationChange", context.Message.Key, _config[context.Message.Key], context.Message.Value]);
        _config[context.Message.Key] = context.Message.Value;
        return Task.CompletedTask;
    }
}