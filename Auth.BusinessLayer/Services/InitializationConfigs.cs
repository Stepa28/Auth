using Auth.BusinessLayer.Helpers;
using Marvelous.Contracts.Enums;
using Marvelous.Contracts.ExchangeModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace Auth.BusinessLayer.Services;

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

    public async Task InitializeConfigs()
    {
        _logger.LogInformation($"Attempt to initialize configs from {Microservice.MarvelousConfigs} service");
        var token = _authService.GetTokenForMicroservice(Microservice.MarvelousAuth);

        RestResponse<IEnumerable<ConfigExchangeModel>> response;
        try
        {
            response = await _requestHelper.SendRequestAsync<IEnumerable<ConfigExchangeModel>>($"https://{_config[Microservice.MarvelousConfigs.ToString()]}",
                "/api/microservices",
                Method.Get,
                Microservice.MarvelousConfigs,
                token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to initialize configs from {Microservice.MarvelousConfigs} service");
            _logger.LogInformation("Start initialize default config");
            for (var i = Microservice.MarvelousAccountChecking; i < Microservice.Undefined; i++)
            {
                _config[i.ToString()] = "::1";
            }
            return;
        }

        _logger.LogInformation($"Start initialize from {Microservice.MarvelousConfigs} service configs");
        foreach (var config in response.Data!)
        {
            _config[config.Key] = config.Value;
        }
    }
}