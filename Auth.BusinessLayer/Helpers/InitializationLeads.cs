using Auth.BusinessLayer.Models;
using Auth.BusinessLayer.Producers;
using Auth.BusinessLayer.Services;
using AutoMapper;
using Marvelous.Contracts.Enums;
using Marvelous.Contracts.ExchangeModels;
using Marvelous.Contracts.Urls;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace Auth.BusinessLayer.Helpers;

public class InitializationLeads : IInitializationLeads
{
    private readonly IRequestHelper _requestHelper;
    private readonly ILogger<InitializationLeads> _logger;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private readonly IAuthProducer _producer;
    private readonly IAuthService _authService;
    private readonly IConfiguration _config;

    public InitializationLeads(IRequestHelper requestHelper, ILogger<InitializationLeads> logger, IMapper mapper, IMemoryCache cache, IAuthProducer producer, IAuthService authService, IConfiguration config)
    {
        _requestHelper = requestHelper;
        _logger = logger;
        _mapper = mapper;
        _cache = cache;
        _producer = producer;
        _authService = authService;
        _config = config;
    }

    public async Task InitializeLeadsAsync()
    {
        _cache.Set("Initialization", false);
        var token = _authService.GetTokenForMicroservice(Microservice.MarvelousAuth);
        
        var response = await GetRestResponse(CrmUrls.LeadApi + CrmUrls.Auth, Microservice.MarvelousCrm, token);
        if (response is null)
        {
            response = await GetRestResponse(ReportingUrls.ApiLeads + ReportingUrls.GetAllLeads, Microservice.MarvelousReporting, token);
            if (response is null)
            {
                var message = $"Initialization with {Microservice.MarvelousCrm} and {Microservice.MarvelousReporting} failed";
                _logger.LogWarning(message);
                await _producer.NotifyFatalError(message);
                return;
            }
            _logger.LogInformation("Initialization from service Reporting: completed successfully");
        }
        else
        {
            _logger.LogInformation("Initialization from service CRM: completed successfully");
        }

        foreach (var entity in response.Data!)
        {
            _cache.Set(entity.Email, _mapper.Map<LeadAuthModel>(entity));
        }
        _cache.Set("Initialization", true);
    }

    private async Task<RestResponse<IEnumerable<LeadAuthExchangeModel>>?> GetRestResponse(string path, Microservice service, string token)
    {
        _logger.LogInformation($"Attempt to initialize from {service} service");
        RestResponse<IEnumerable<LeadAuthExchangeModel>>? response = null;
        try
        {
            response = await _requestHelper.SendRequestAsync<IEnumerable<LeadAuthExchangeModel>>(_config[$"{service}Url"], path, Method.Get, service, token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to initialize from {service} service ({ex.Message})");
        }

        return response;
    }
}