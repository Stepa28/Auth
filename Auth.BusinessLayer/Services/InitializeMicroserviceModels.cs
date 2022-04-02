using Auth.BusinessLayer.Exceptions;
using Auth.BusinessLayer.Models;
using Marvelous.Contracts.Enums;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Auth.BusinessLayer.Services;

public class InitializeMicroserviceModels
{
    private const string Front = "Front"; //TODO убрать магические числа
    private readonly ILogger _logger;

    public InitializeMicroserviceModels(ILogger logger)
    {
        _logger = logger;
    }

    public Dictionary<Microservice, MicroserviceModel> InitializeMicroservices(ICacheEntry _)
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
            new MicroserviceModel("::1", () => string.Join(",", Microservice.TransactionStore.ToString(), Front), Microservice.CRM));
        
        microservices.Add(Microservice.MarvelousReportMicroService,
            new MicroserviceModel("", () => Front, Microservice.MarvelousReportMicroService));

        return microservices;
    }

    private void ForbiddenMicroservices(IDictionary<Microservice, MicroserviceModel> microservices, params Microservice[] services)
    {
        foreach (var service in services)
        {
            string Forbidden()
            {
                var ex = new ForbiddenException($"{service.ToString()} service does not have the right to issue a token");
                _logger.LogError(ex, "");
                throw ex;
            }
            microservices.Add(service, new MicroserviceModel("", Forbidden, service));
        }
    }
}