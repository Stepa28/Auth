using System;
using Microsoft.Extensions.Logging;
using Moq;
using static Moq.It;

namespace Auth.BusinessLayer.Test;

internal static class LoggerVerifyHelper
{
    internal static void Verify<T>(Mock<ILogger<T>> logger, LogLevel level, int times)
    {
        logger.Verify(v => v.Log(level,
                IsAny<EventId>(),
                Is<IsAnyType>((o, t) => true),
                IsAny<Exception>(),
                IsAny<Func<IsAnyType, Exception, string>>()!),
            Times.Exactly(times));
    }
}