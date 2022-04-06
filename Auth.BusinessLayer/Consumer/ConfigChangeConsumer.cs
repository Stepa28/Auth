using Auth.BusinessLayer.Services;
using Marvelous.Contracts.Configurations;
using Marvelous.Contracts.Enums;
using MassTransit;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Auth.BusinessLayer.Consumer;

public class ConfigChangeConsumer : IConsumer<AuthCfg>
{
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _config;
    private readonly ILogger<ConfigChangeConsumer> _logger;
    private readonly IInitializeMicroserviceModels _initializeMicroservice;

    public ConfigChangeConsumer(IMemoryCache cache, ILogger<ConfigChangeConsumer> logger, IConfiguration config, IInitializeMicroserviceModels initializeMicroservice)
    {
        _cache = cache;
        _logger = logger;
        _config = config;
        _initializeMicroservice = initializeMicroservice;
    }

    public Task Consume(ConsumeContext<AuthCfg> context)
    {
        _logger.LogInformation($"Configuration {context.Message.Key} change value {_config[context.Message.Key]} to {context.Message.Value}");
        _config[context.Message.Key] = context.Message.Value;
        _cache.Set(nameof(Microservice), _initializeMicroservice.InitializeMicroservices());
        return Task.CompletedTask;
    }
}