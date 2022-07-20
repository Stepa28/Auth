using System.Collections.Generic;
using System.Threading.Tasks;
using Auth.BusinessLayer.Consumer;
using Auth.BusinessLayer.Validators;
using FluentValidation;
using Marvelous.Contracts.Configurations;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Auth.BusinessLayer.Test;

public class ConfigChangeConsumerTests : VerifyHelper
{

    #region SetUp

    #pragma warning disable CS8618
    private IConfiguration _config;
    private IConsumer<AuthCfg> _consumer;
    private Mock<ILogger<ConfigChangeConsumer>> _logger;
    private IValidator<AuthCfg> _validator;
    #pragma warning restore CS8618

    [SetUp]
    public void SetUp()
    {
        _config = new ConfigurationBuilder().AddJsonFile("appsettings.Test.json").AddInMemoryCollection(new Dictionary<string, string>()).Build();
        _logger = new Mock<ILogger<ConfigChangeConsumer>>();
        _validator = new AuthCfgValidator();

        _consumer = new ConfigChangeConsumer(_logger.Object, _config, _validator, _localizer.Object);
    }

    #endregion

    #region Consume

    [Test]
    public async Task Consume_ValidRequestReceived_ShouldApplyChanges()
    {
        //given
        var cfg = new AuthCfg { Key = "BaseAddress", Value = "testAddress" };
        var context = Mock.Of<ConsumeContext<AuthCfg>>(_ => _.Message == cfg);

        //when
        await _consumer.Consume(context);

        //then
        Assert.AreEqual(_config[cfg.Key], cfg.Value);
        VerifyLogger(_logger, LogLevel.Information, 1);
    }

    [Test]
    public void Consume_FailedValidate_ShouldThrowValidationException()
    {
        //given
        var cfg = new AuthCfg();
        var expected =
            "Validation failed: \r\n -- Key: 'Key' должно быть заполнено. Severity: Error\r\n -- Value: 'Value' должно быть заполнено. Severity: Error";
        var context = Mock.Of<ConsumeContext<AuthCfg>>(_ => _.Message == cfg);

        //when
        var actual = Assert.Throws<ValidationException>(() => _consumer.Consume(context))?.Message;

        //then
        Assert.AreEqual(expected, actual);
        VerifyLogger(_logger, It.IsAny<LogLevel>(), 0);
    }

    #endregion

}