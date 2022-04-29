using System;
using System.Collections.Generic;
using Auth.API.Extensions;
using Marvelous.Contracts.Client;
using Marvelous.Contracts.Enums;
using Marvelous.Contracts.ResponseModels;
using Microsoft.Extensions.Logging;
using Moq;
using RestSharp;
using static Moq.It;

namespace Auth.BusinessLayer.Test;

public abstract class VerifyHelper
{
    protected static void VerifyLogger<T>(Mock<ILogger<T>> logger, LogLevel level, int times)
    {
        logger.Verify(v => v.Log(level,
                IsAny<EventId>(),
                Is<IsAnyType>((o, t) => true),
                IsAny<Exception>(),
                IsAny<Func<IsAnyType, Exception, string>>()!),
            Times.Exactly(times));
    }

    protected static void VerifyRequestHelperTests(Mock<IRestClient> client)
    {
        client.Verify(v => v.AddMicroservice(Microservice.MarvelousAuth), Times.Once);
        client.Verify(v => v.ExecuteAsync<IEnumerable<ConfigResponseModel>>(IsAny<RestRequest>(), default), Times.Once);
    }

    protected static void VerifyAdvancedController(Mock<IAdvancedController> advancedController, int service, int issuer, int audience, int identity)
    {
        advancedController.Verify(v => v.Service, Times.Exactly(service));
        advancedController.Verify(v => v.Issuer, Times.Exactly(issuer));
        advancedController.Verify(v => v.Audience, Times.Exactly(audience));
        advancedController.Verify(v => v.Identity, Times.Exactly(identity));
    }

    protected static void SetupAdvancedController(Mock<IAdvancedController> advancedController, Microservice service, Microservice audience)
    {
        advancedController.Setup(s => s.Service).Returns(service);
        advancedController.Setup(s => s.Issuer).Returns(service.ToString());
        advancedController.Setup(s => s.Audience).Returns(audience.ToString());
    }
}