using Auth.BusinessLayer.Models;
using Marvelous.Contracts.Enums;
using Microsoft.Extensions.Configuration;

namespace Auth.BusinessLayer.Services;

public class InitializeMicroserviceModels : IInitializeMicroserviceModels
{
    private readonly IConfiguration _config;

    public InitializeMicroserviceModels(IConfiguration config)
    {
        _config = config;
    }

    public Dictionary<Microservice, MicroserviceModel> InitializeMicroservices()
    {
        var microservices = new Dictionary<Microservice, MicroserviceModel>();
        OnlyConfigMicroservices(microservices,
            Microservice.MarvelousConfigs,
            Microservice.MarvelousRatesApi,
            Microservice.MarvelousEmailSendler,
            Microservice.MarvelousAccountChecking);

        microservices.Add(Microservice.MarvelousResource,
            new MicroserviceModel(_config[Microservice.MarvelousResource.ToString()],
                StringJoinMicroservices(Microservice.MarvelousTransactionStore, Microservice.MarvelousCrm, Microservice.MarvelousFrontendService),
                Microservice.MarvelousResource,
                Microservice.MarvelousFrontendService));

        microservices.Add(Microservice.MarvelousTransactionStore,
            new MicroserviceModel(_config[Microservice.MarvelousTransactionStore.ToString()],
                Microservice.MarvelousCrm.ToString(),
                Microservice.MarvelousTransactionStore,
                Microservice.Undefined));

        microservices.Add(Microservice.MarvelousCrm,
            new MicroserviceModel(_config[Microservice.MarvelousCrm.ToString()],
                StringJoinMicroservices(Microservice.MarvelousTransactionStore, Microservice.MarvelousAuth, Microservice.MarvelousFrontendCrm),
                Microservice.MarvelousCrm,
                Microservice.MarvelousFrontendCrm));

        microservices.Add(Microservice.MarvelousReporting,
            new MicroserviceModel(_config[Microservice.MarvelousReporting.ToString()],
                Microservice.MarvelousFrontendReporting.ToString(),
                Microservice.MarvelousReporting,
                Microservice.MarvelousFrontendReporting));

        microservices.Add(Microservice.MarvelousAuth,
            new MicroserviceModel(_config[Microservice.MarvelousAuth.ToString()],
                StringJoinMicroservices(Microservice.MarvelousCrm, Microservice.MarvelousReporting),
                Microservice.MarvelousAuth,
                Microservice.Undefined));

        return microservices;
    }

    private void OnlyConfigMicroservices(IDictionary<Microservice, MicroserviceModel> microservices, params Microservice[] services)
    {
        foreach (var service in services)
        {
            microservices.Add(service,
                new MicroserviceModel(_config[service.ToString()], Microservice.MarvelousConfigs.ToString(), service, Microservice.Undefined));
        }
    }

    private static string StringJoinMicroservices(params Microservice[] services)
    {
        var listServices = services.ToList();
        listServices.Add(Microservice.MarvelousConfigs);
        return string.Join(',', listServices);
    }
}