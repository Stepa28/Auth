using Auth.BusinessLayer.Models;
using Auth.BusinessLayer.Producers;
using Auth.BusinessLayer.Services;
using AutoMapper;
using Marvelous.Contracts.Endpoints;
using Marvelous.Contracts.Enums;
using Marvelous.Contracts.ExchangeModels;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
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

    public InitializationLeads(IRequestHelper<LeadAuthExchangeModel> requestHelper, ILogger<InitializationLeads> logger, IMapper mapper, IMemoryCache cache,
        IAuthProducer producer, IAuthService authService, IConfiguration config)
    {
        _requestHelper = requestHelper;
        _logger = logger;
        _mapper = mapper;
        _cache = cache;
        _producer = producer;
        _authService = authService;
        _config = config;
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
                var message = $"Initialization leads with {Microservice.MarvelousCrm} and {Microservice.MarvelousReporting} failed";
                _logger.LogWarning(message);
                await _producer.NotifyErrorByEmail(message);
                return;
            }
            _logger.LogInformation("Response from service Reporting: received successfully");
        }
        else
        {
            _logger.LogInformation("Response from service CRM: received successfully");
        }

        foreach (var entity in response.Data!)
            _cache.Set(entity.Email, _mapper.Map<LeadAuthModel>(entity));
        _logger.LogInformation("Initialization leads: completed successfully");
        _cache.Set("Initialization leads", true);
    }

    private async Task<RestResponse<IEnumerable<LeadAuthExchangeModel>>?> GetRestResponse(string path, Microservice service, string token)
    {
        _logger.LogInformation($"Attempt to initialize from {service} service");
        RestResponse<IEnumerable<LeadAuthExchangeModel>>? response = null;
        try
        {
            response = await _requestHelper.SendRequest($"{_config[$"{service}Url"]}{path}", service, token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to initialize from {service} service ({ex.Message})");
        }

        return response;
    }
}