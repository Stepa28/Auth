using System;
using System.Collections.Generic;
using Auth.BusinessLayer.Helpers;
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
}