using Auth.BusinessLayer.Exceptions;
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
    private const string Front = "Front"; //TODO убрать магические числа
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
        InitializeMicroservices();
        var response = GetRestResponse(CrmUrls.Url, CrmUrls.Api + CrmUrls.Auth, Microservice.CRM).Result;
        if (response is null)
        {
            response = GetRestResponse(ReportingUrls.Url, ReportingUrls.ApiLeads + ReportingUrls.Auth, Microservice.MarvelousReportMicroService).Result;
            if (response is null)
            {
                var message =
                    $"Initialization with {Microservice.CRM} and {Microservice.MarvelousReportMicroService} failed";
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

    private void InitializeMicroservices()
    {
        var microservices = new Dictionary<Microservice, MicroserviceModel>();
        ForbiddenMicroservices(microservices,
            Microservice.Auth,
            Microservice.MarvelousConfigs,
            Microservice.RatesApi,
            Microservice.workerServiceEmail,
            Microservice.MarvelousAccountCheckingByChuZhig);

        microservices.Add(Microservice.MarvelousService,
            new MicroserviceModel("",
                () => string.Join(",", Microservice.TransactionStore.ToString(), Microservice.CRM.ToString(), Front)!,
                Microservice.MarvelousService));
        
        microservices.Add(Microservice.TransactionStore,
            new MicroserviceModel("", () => Microservice.CRM.ToString(), Microservice.TransactionStore));
        
        microservices.Add(Microservice.CRM,
            new MicroserviceModel("172.16.0.67", () => string.Join(",", Microservice.TransactionStore.ToString(), Front), Microservice.CRM));
        
        microservices.Add(Microservice.MarvelousReportMicroService,
            new MicroserviceModel("172.16.0.232", () => Front, Microservice.MarvelousReportMicroService));

        _cache.Set(nameof(Microservice), microservices);
    }

    private void ForbiddenMicroservices(IDictionary<Microservice, MicroserviceModel> microservices, params Microservice[] services)
    {
        foreach (var service in services)
        {
            string Forbidden() => throw new ForbiddenException($"{service.ToString()} service does not have the right to issue a token");
            microservices.Add(service, new MicroserviceModel("", Forbidden, service));
        }
    }
}