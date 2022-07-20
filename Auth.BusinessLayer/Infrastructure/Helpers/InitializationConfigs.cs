using Auth.BusinessLayer.Producers;
using Auth.BusinessLayer.Services;
using Auth.Resources;
using Marvelous.Contracts.Endpoints;
using Marvelous.Contracts.Enums;
using Marvelous.Contracts.ResponseModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace Auth.BusinessLayer.Helpers;

public class InitializationConfigs : IInitializationConfigs
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _config;
    private readonly ILogger<InitializationConfigs> _logger;
    private readonly IAuthProducer _producer;
    private readonly IRequestHelper<ConfigResponseModel> _requestHelper;
    private readonly IStringLocalizer<ExceptionAndLogMessages> _localizer;

    public InitializationConfigs(IConfiguration config, ILogger<InitializationConfigs> logger, IRequestHelper<ConfigResponseModel> requestHelper,
        IAuthService authService, IAuthProducer producer, IStringLocalizer<ExceptionAndLogMessages> localizer)
    {
        _config = config;
        _logger = logger;
        _requestHelper = requestHelper;
        _authService = authService;
        _producer = producer;
        _localizer = localizer;
    }

    public async Task InitializeConfigs()
    {
        var token = _authService.GetTokenForMicroservice(Microservice.MarvelousAuth);

        try
        {
            _logger.LogInformation(_localizer["AttemptInitializeConfigs", Microservice.MarvelousConfigs]);
            var response = await _requestHelper.SendRequest($"{_config[$"{Microservice.MarvelousConfigs}Url"]}{ConfigsEndpoints.Configs}",
                Microservice.MarvelousConfigs,
                token);

            foreach (var config in response.Data!)
                _config[config.Key] = config.Value;
            _logger.LogInformation(_localizer["InitializeSuccessfully", Microservice.MarvelousConfigs]);
        }
        catch (Exception ex)
        {
            var message = _localizer["FailedInitializeConfigs", Microservice.MarvelousConfigs, ex.Message].ToString();
            _logger.LogWarning(ex, message);
            await _producer.NotifyErrorByEmail(message);
        }
    }
}