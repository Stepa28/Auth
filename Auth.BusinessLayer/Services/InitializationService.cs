using Auth.BusinessLayer.Helpers;
using Auth.BusinessLayer.Models;
using Auth.BusinessLayer.Producers;
using AutoMapper;
using Marvelous.Contracts.Enums;
using Marvelous.Contracts.ExchangeModels;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Marvelous.Contracts.Urls;
using Newtonsoft.Json;
using RestSharp;
using Timer = System.Timers.Timer;

namespace Auth.BusinessLayer.Services;

public class InitializationService
{
    private readonly IRequestHelper _requestHelper;
    private readonly ILogger<InitializationService> _logger;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private readonly IAuthProducer _producer;
    private readonly IAuthService _authService;

    public InitializationService(IRequestHelper requestHelper, ILogger<InitializationService> logger, IMapper mapper, IMemoryCache cache, IAuthProducer producer, IAuthService authService)
    {
        _requestHelper = requestHelper;
        _logger = logger;
        _mapper = mapper;
        _cache = cache;
        _producer = producer;
        _authService = authService;
    }

    public async Task InitializeMemoryCashAsync(Timer timer)
    {
        _cache.Set("Initialization", false);
        var token = await _authService.GetTokenForMicroservice(Microservice.MarvelousAuth);
        
        var response = await GetRestResponse(CrmUrls.Url, CrmUrls.LeadApi + CrmUrls.Auth, Microservice.MarvelousCrm, token);
        if (response is null)
        {
            response = await GetRestResponse(ReportingUrls.Url, ReportingUrls.ApiLeads + ReportingUrls.Auth, Microservice.MarvelousReporting, token);
            if (response is null)
            {
                var message = $"Initialization with {Microservice.MarvelousCrm} and {Microservice.MarvelousReporting} failed";
                _logger.LogCritical(message);
                await _producer.NotifyFatalError(message);
                timer.Start();
                return;
            }
            _logger.LogInformation("Initialization from service Reporting: completed successfully");
        }
        else
        {
            _logger.LogInformation("Initialization from service CRM: completed successfully");
        }

        foreach (var entity in JsonConvert.DeserializeObject<IEnumerable<LeadAuthExchangeModel>>(response.Content!)!)
        {
            _cache.Set(entity.Email, _mapper.Map<LeadAuthModel>(entity));
        }
        _cache.Set("Initialization", true);
    }

    private async Task<RestResponse?> GetRestResponse(string url, string path, Microservice service, string token)
    {
        _logger.LogInformation($"Attempt to initialize from {service} service");
        RestResponse? response = null;
        try
        {
            response = await _requestHelper.SendRequest(url, path, Method.Get, token, service);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to initialize from {service} service");
        }

        return response;
    }
}