using Auth.BusinessLayer.Exceptions;
using Auth.BusinessLayer.Models;
using Marvelous.Contracts.Enums;
using Microsoft.Extensions.Logging;

namespace Auth.BusinessLayer.Services;

public class InitializeMicroserviceModels : IInitializeMicroserviceModels
{
    private readonly ILogger<InitializeMicroserviceModels> _logger;

    public InitializeMicroserviceModels(ILogger<InitializeMicroserviceModels> logger)
    {
        _logger = logger;
    }

    public Dictionary<Microservice, MicroserviceModel> InitializeMicroservices()
    {
        var microservices = new Dictionary<Microservice, MicroserviceModel>();
        ForbiddenMicroservices(microservices,
            Microservice.MarvelousConfigs,
            Microservice.MarvelousRatesApi,
            Microservice.MarvelousEmailSendler,
            Microservice.MarvelousAccountChecking);

        microservices.Add(Microservice.MarvelousService,
            new MicroserviceModel("",
                () => string.Join(",",
                    Microservice.MarvelousTransactionStore.ToString(),
                    Microservice.MarvelousCrm.ToString(),
                    Frontend.MarvelousFrontendService.ToString()),
                Microservice.MarvelousService) { Frontend = Frontend.MarvelousFrontendService });

        microservices.Add(Microservice.MarvelousTransactionStore,
            new MicroserviceModel("", () => Microservice.MarvelousCrm.ToString(), Microservice.MarvelousTransactionStore));

        microservices.Add(Microservice.MarvelousCrm,
            new MicroserviceModel("::1",
                () => string.Join(",",
                    Microservice.MarvelousTransactionStore.ToString(),
                    Microservice.MarvelousAuth.ToString(),
                    Frontend.MarvelousFrontendCrm.ToString()),
                Microservice.MarvelousCrm) { Frontend = Frontend.MarvelousFrontendCrm });

        microservices.Add(Microservice.MarvelousReporting,
            new MicroserviceModel("", () => Frontend.MarvelousFrontendReporting.ToString(), Microservice.MarvelousReporting)
                { Frontend = Frontend.MarvelousFrontendReporting });

        microservices.Add(Microservice.MarvelousAuth,
            new MicroserviceModel("",
                () => string.Join(",", Microservice.MarvelousCrm.ToString(), Microservice.MarvelousReporting.ToString()),
                Microservice.MarvelousAuth));

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