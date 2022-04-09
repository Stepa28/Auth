using Auth.BusinessLayer.Services;
using Marvelous.Contracts.Endpoints;
using Marvelous.Contracts.Enums;
using Marvelous.Contracts.ResponseModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Auth.BusinessLayer.Helpers;

public class InitializationConfigs : IInitializationConfigs
{
    private readonly IConfiguration _config;
    private readonly ILogger<InitializationConfigs> _logger;
    private readonly IRequestHelper _requestHelper;
    private readonly IAuthService _authService;

    public InitializationConfigs(IConfiguration config, ILogger<InitializationConfigs> logger, IRequestHelper requestHelper, IAuthService authService)
    {
        _config = config;
        _logger = logger;
        _requestHelper = requestHelper;
        _authService = authService;
    }

    public void InitializeConfigs()
    {
        _logger.LogInformation($"Attempt to initialize configs from {Microservice.MarvelousConfigs} service");
        _logger.LogInformation("Initialize from default config");
        _config["BaseAddress"] = "80.78.240.16";
        _config[$"{Microservice.MarvelousCrm}Url"] = "https://piter-education.ru:5050";
        _config[$"{Microservice.MarvelousReporting}Url"] = "https://piter-education.ru:6010";

        var token = _authService.GetTokenForMicroservice(Microservice.MarvelousAuth);

        try
        {
            var response = _requestHelper.SendRequest<IEnumerable<ConfigResponseModel>>(_config[$"{Microservice.MarvelousConfigs}Url"],
                ConfigsEndpoints.Configs,
                Microservice.MarvelousConfigs,
                token).Result;

            _logger.LogInformation($"Start initialize from {Microservice.MarvelousConfigs} service configs");
            foreach (var config in response.Data!)
            {
                _config[config.Key] = config.Value;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Failed to initialize configs from {Microservice.MarvelousConfigs} service({ex.Message})");
        }
    }
}