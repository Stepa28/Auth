using Auth.BusinessLayer.Helpers;
using Auth.BusinessLayer.Models;
using AutoMapper;
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

    public InitializationService(IRequestHelper requestHelper, ILogger<InitializationService> logger, IMapper mapper, IMemoryCache cache)
    {
        _requestHelper = requestHelper;
        _logger = logger;
        _mapper = mapper;
        _cache = cache;
    }

    public void InitializeMamoryCash()
    {
        var response = GetRestResponse("https://localhost:7295", MarvelousServices.CRM).Result;
        if (response is null)
        {
            //TODO договориться с Reporting по поводу url и EP
            //response = await GetRestResponse(CrmUrls.Url, MarvelousServices.Reporting);
            if (response is null)
                //TODO что-то сделать
                return;
            else
                _logger.LogInformation("Initialization from service Reporting: completed successfully");
        }
        else
            _logger.LogInformation("Initialization from service CRM: completed successfully");

        var leads = JsonConvert.DeserializeObject<IEnumerable<LeadAuthExchangeModel>>(response.Content!);
        var dictionary = leads!.ToDictionary(lead => lead.Email, lead => _mapper.Map<LeadAuthModel>(lead));
        _cache.Set("leads", dictionary);
    }

    private async Task<RestResponse?> GetRestResponse(string url, MarvelousServices service)
    {
        _logger.LogInformation($"Attempt to initialize from {service} service");
        RestResponse? response = null;
        try
        {
            response = await _requestHelper.SendRequest(url, CrmUrls.Api + CrmUrls.Auth, Method.Get, service);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to initialize from {service} service");
        }

        return response;
    }
}