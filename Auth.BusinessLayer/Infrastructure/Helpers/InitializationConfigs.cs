using Auth.BusinessLayer.Producers;
using Auth.BusinessLayer.Services;
using Marvelous.Contracts.Endpoints;
using Marvelous.Contracts.Enums;
using Marvelous.Contracts.ResponseModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Auth.BusinessLayer.Helpers;

public class InitializationConfigs : IInitializationConfigs
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _config;
    private readonly ILogger<InitializationConfigs> _logger;
    private readonly IAuthProducer _producer;
    private readonly IRequestHelper<ConfigResponseModel> _requestHelper;

    public InitializationConfigs(IConfiguration config, ILogger<InitializationConfigs> logger, IRequestHelper<ConfigResponseModel> requestHelper,
        IAuthService authService, IAuthProducer producer)
    {
        _config = config;
        _logger = logger;
        _requestHelper = requestHelper;
        _authService = authService;
        _producer = producer;
    }

    public void InitializeConfigs()
    {
        _logger.LogInformation("Initialize from default config");
        _config["BaseAddress"] = "80.78.240.16";
        _config[$"{Microservice.MarvelousCrm}Url"] = "https://piter-education.ru:5050";
        _config[$"{Microservice.MarvelousReporting}Url"] = "https://piter-education.ru:6010";

        var token = _authService.GetTokenForMicroservice(Microservice.MarvelousAuth);

        try
        {
            _logger.LogInformation($"Attempt to initialize configs from {Microservice.MarvelousConfigs} service");
            var response = _requestHelper.SendRequest(_config[$"{Microservice.MarvelousConfigs}Url"],
                ConfigsEndpoints.Configs,
                Microservice.MarvelousConfigs,
                token).Result;

            foreach (var config in response.Data!)
            {
                _config[config.Key] = config.Value;
            }
            _logger.LogInformation($"Initialize from {Microservice.MarvelousConfigs} service: completed successfully");
        }
        catch (Exception ex)
        {
            var message = $"Failed to initialize configs from {Microservice.MarvelousConfigs} service({ex.Message})";
            _logger.LogWarning(ex, message);
            _producer.NotifyErrorByEmail(message);
        }
    }
}