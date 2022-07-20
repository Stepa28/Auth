using System.Threading.Tasks;
using Auth.BusinessLayer.Consumer;
using Auth.BusinessLayer.Models;
using Auth.BusinessLayer.Validators;
using FluentValidation;
using Marvelous.Contracts.Enums;
using Marvelous.Contracts.ExchangeModels;
using MassTransit;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Auth.BusinessLayer.Test;

public class AccountCheckingChangeRoleConsumerTests : VerifyHelper
{

    #region SetUp

    #pragma warning disable CS8618
    private IMemoryCache _cache;
    private IConsumer<LeadShortExchangeModel[]> _consumer;
    private Mock<ILogger<AccountCheckingChangeRoleConsumer>> _logger;
    private IValidator<LeadShortExchangeModel> _validator;
    #pragma warning restore CS8618

    [SetUp]
    public void SetUp()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _logger = new Mock<ILogger<AccountCheckingChangeRoleConsumer>>();
        _validator = new LeadShortExchangeModelValidator();

        _consumer = new AccountCheckingChangeRoleConsumer(_logger.Object, _cache, _validator, _localizer.Object);

        _cache.Set("test@example.com", new LeadAuthModel { HashPassword = "1000", Role = Role.Vip, Id = 2 });
        _cache.Set("test4@example.com", new LeadAuthModel { HashPassword = "4000", Role = Role.Vip, Id = 3 });
    }

    #endregion

    #region Consume

    [Test]
    public async Task Consume_ValidRequestReceived_ShouldApplyChanges()
    {
        //given
        var models = new LeadShortExchangeModel[]
        {
            new() { Email = "test@example.com", Role = Role.Regular, Id = 5 },
            new() { Email = "test5@example.com", Role = Role.Regular, Id = 10 }
        };
        var context = Mock.Of<ConsumeContext<LeadShortExchangeModel[]>>(_ => _.Message == models);

        //when
        await _consumer.Consume(context);

        //then
        Assert.AreEqual(_cache.Get<LeadAuthModel>(models[0].Email).Role, models[0].Role);
        Assert.AreEqual(_cache.Get<LeadAuthModel>(models[0].Email).Id, 2);
        Assert.AreEqual(_cache.Get<LeadAuthModel>(models[0].Email).HashPassword, "1000");
        Assert.AreEqual(_cache.Get<LeadAuthModel>("test4@example.com").Role, Role.Vip);
        Assert.IsTrue(_cache.Get<LeadAuthModel>(models[1].Email).Id == default);
        Assert.IsTrue(_cache.Get<LeadAuthModel>(models[1].Email).Role == default);
        VerifyLogger(_logger, LogLevel.Information, 1);
    }

    [Test]
    public void Consume_FailedValidate_ShouldThrowValidationException()
    {
        //given
        var models = new LeadShortExchangeModel[]
        {
            new() { Email = "", Id = 5 },
            new() { Email = "", Role = Role.Regular, Id = 10 }
        };
        var expected =
            "Validation failed: \r\n -- Email: 'Email' должно быть заполнено. Severity: Error\r\n -- Role: 'Role' должно быть заполнено. Severity: Error";
        var context = Mock.Of<ConsumeContext<LeadShortExchangeModel[]>>(_ => _.Message == models);

        //when
        var actual = Assert.Throws<ValidationException>(() => _consumer.Consume(context))?.Message;

        //then
        Assert.AreEqual(expected, actual);
        VerifyLogger(_logger, It.IsAny<LogLevel>(), 0);
    }

    #endregion

}