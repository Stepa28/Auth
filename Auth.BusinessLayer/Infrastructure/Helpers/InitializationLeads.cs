using Auth.BusinessLayer.Models;
using Auth.BusinessLayer.Producers;
using Auth.BusinessLayer.Services;
using Auth.Resources;
using AutoMapper;
using Marvelous.Contracts.Endpoints;
using Marvelous.Contracts.Enums;
using Marvelous.Contracts.ExchangeModels;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace Auth.BusinessLayer.Helpers;

public class InitializationLeads : IInitializationLeads
{
    private readonly IAuthService _authService;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _config;
    private readonly ILogger<InitializationLeads> _logger;
    private readonly IMapper _mapper;
    private readonly IAuthProducer _producer;
    private readonly IRequestHelper<LeadAuthExchangeModel> _requestHelper;
    private readonly IStringLocalizer<ExceptionAndLogMessages> _localizer;

    public InitializationLeads(IRequestHelper<LeadAuthExchangeModel> requestHelper, ILogger<InitializationLeads> logger, IMapper mapper, IMemoryCache cache,
        IAuthProducer producer, IAuthService authService, IConfiguration config, IStringLocalizer<ExceptionAndLogMessages> localizer)
    {
        _requestHelper = requestHelper;
        _logger = logger;
        _mapper = mapper;
        _cache = cache;
        _producer = producer;
        _authService = authService;
        _config = config;
        _localizer = localizer;
    }

    public async Task InitializeLeads()
    {
        _cache.Set("Initialization leads", false);
        var token = _authService.GetTokenForMicroservice(Microservice.MarvelousAuth);

        var response = await GetRestResponse(CrmEndpoints.LeadApi + CrmEndpoints.Auth, Microservice.MarvelousCrm, token);
        if (response is null)
        {
            response = await GetRestResponse(ReportingEndpoints.ApiLeads + ReportingEndpoints.GetAllLeads, Microservice.MarvelousReporting, token);
            if (response is null)
            {
                var message = _localizer["FailedInitializationLeads", Microservice.MarvelousCrm , Microservice.MarvelousReporting];
                _logger.LogWarning(message);
                await _producer.NotifyErrorByEmail(message);
                return;
            }
            _logger.LogInformation(_localizer["ResponseSuccessfully", Microservice.MarvelousReporting]);
        }
        else
        {
            _logger.LogInformation(_localizer["ResponseSuccessfully", Microservice.MarvelousCrm]);
        }

        foreach (var entity in response.Data!)
            _cache.Set(entity.Email, _mapper.Map<LeadAuthModel>(entity));
        _logger.LogInformation(_localizer["InitializeLeadsSuccessfully"]);
        _cache.Set("Initialization leads", true);
    }

    private async Task<RestResponse<IEnumerable<LeadAuthExchangeModel>>?> GetRestResponse(string path, Microservice service, string token)
    {
        _logger.LogInformation(_localizer["AttemptInitialize", service]);
        RestResponse<IEnumerable<LeadAuthExchangeModel>>? response = null;
        try
        {
            response = await _requestHelper.SendRequest($"{_config[$"{service}Url"]}{path}", service, token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer["FailedInitializeConfigs", service, ex.Message]);
        }

        return response;
    }
}