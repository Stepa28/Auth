using System;
using System.Threading;
using System.Threading.Tasks;
using Auth.BusinessLayer.Producers;
using Marvelous.Contracts.EmailMessageModels;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Auth.BusinessLayer.Test;

public class AuthProducerTests : VerifyHelper
{

    #region SetUp

    #pragma warning disable CS8618
    private Mock<IBus> _bus;
    private Mock<ILogger<AuthProducer>> _logger;
    private IAuthProducer _producer;
    #pragma warning restore CS8618

    [SetUp]
    public void SetUp()
    {
        _bus = new Mock<IBus>();
        _logger = new Mock<ILogger<AuthProducer>>();

        _producer = new AuthProducer(_bus.Object, _logger.Object);
    }

    #endregion


    [Test]
    public async Task NotifyErrorByEmail_ValidRequestReceived_ShouldPublishMassage()
    {
        //given
        var massage = "Test message";

        //when
        await _producer.NotifyErrorByEmail(massage);

        //then
        _bus.Verify(v => v.Publish(It.IsAny<EmailErrorMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        VerifyLogger(_logger, LogLevel.Information, 2);
    }
}