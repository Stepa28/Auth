using Auth.BusinessLayer.Models;
using Marvelous.Contracts.Enums;

namespace Auth.BusinessLayer.Helpers;

public static class InitializeMicroserviceModels
{
    public static Dictionary<Microservice, MicroserviceModel> InitializeMicroservices()
    {
        var microservices = new Dictionary<Microservice, MicroserviceModel>();
        OnlyConfigMicroservices(microservices,
            Microservice.MarvelousRatesApi,
            Microservice.MarvelousEmailSender,
            Microservice.MarvelousSmsSender,
            Microservice.MarvelousAccountChecking,
            Microservice.MarvelousTransactionStore);

        microservices.Add(Microservice.MarvelousResource,
            new MicroserviceModel(
                StringJoinMicroservices(Microservice.MarvelousTransactionStore, Microservice.MarvelousCrm, Microservice.MarvelousFrontendResource),
                Microservice.MarvelousResource,
                Microservice.MarvelousFrontendResource));

        microservices.Add(Microservice.MarvelousCrm,
            new MicroserviceModel(StringJoinMicroservices(Microservice.MarvelousTransactionStore, Microservice.MarvelousAuth, Microservice.MarvelousFrontendCrm),
                Microservice.MarvelousCrm,
                Microservice.MarvelousFrontendCrm));

        microservices.Add(Microservice.MarvelousReporting,
            new MicroserviceModel(Microservice.MarvelousFrontendReporting.ToString(),
                Microservice.MarvelousReporting,
                Microservice.MarvelousFrontendReporting));

        microservices.Add(Microservice.MarvelousAuth,
            new MicroserviceModel(StringJoinMicroservices(Microservice.MarvelousCrm, Microservice.MarvelousReporting),
                Microservice.MarvelousAuth,
                Microservice.MarvelousFrontendUndefined));

        microservices.Add(Microservice.MarvelousConfigs,
            new MicroserviceModel(Microservice.MarvelousFrontendConfigs.ToString(),
                Microservice.MarvelousConfigs,
                Microservice.MarvelousFrontendConfigs));

        return microservices;
    }

    private static void OnlyConfigMicroservices(IDictionary<Microservice, MicroserviceModel> microservices, params Microservice[] services)
    {
        foreach (var service in services)
        {
            microservices.Add(service,
                new MicroserviceModel(Microservice.MarvelousConfigs.ToString(), service, Microservice.MarvelousFrontendUndefined));
        }
    }

    private static string StringJoinMicroservices(params Microservice[] services)
    {
        var listServices = services.ToList();
        listServices.Add(Microservice.MarvelousConfigs);
        return string.Join(',', listServices);
    }
}