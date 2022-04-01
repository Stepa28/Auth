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

namespace Auth.BusinessLayer.Services;

public class InitializationService
{
    private readonly IRequestHelper _requestHelper;
    private readonly ILogger<InitializationService> _logger;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private readonly IAuthProducer _producer;

    public InitializationService(IRequestHelper requestHelper, ILogger<InitializationService> logger, IMapper mapper, IMemoryCache cache, IAuthProducer producer)
    {
        _requestHelper = requestHelper;
        _logger = logger;
        _mapper = mapper;
        _cache = cache;
        _producer = producer;
    }

    public void InitializeMamoryCash()
    {
        var response = GetRestResponse(CrmUrls.Url, CrmUrls.Api + CrmUrls.Auth, Microservice.CRM).Result;
        if (response is null)
        {
            response = GetRestResponse(ReportingUrls.Url, ReportingUrls.ApiLeads + ReportingUrls.Auth, Microservice.MarvelousReportMicroService).Result;
            if (response is null)
            {
                var message = $"Initialization with {Microservice.CRM} and {Microservice.MarvelousReportMicroService} failed";
                _logger.LogError(message);
                _producer.NotifyFatalError(message);
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
    }

    private async Task<RestResponse?> GetRestResponse(string url, string path, Microservice service)
    {
        _logger.LogInformation($"Attempt to initialize from {service} service");
        RestResponse? response = null;
        try
        {
            response = await _requestHelper.SendRequest(url, path, Method.Get, service);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to initialize from {service} service");
        }

        return response;
    }
}