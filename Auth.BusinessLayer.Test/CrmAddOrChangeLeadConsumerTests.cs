using System.Threading.Tasks;
using Auth.BusinessLayer.Configuration;
using Auth.BusinessLayer.Consumer;
using Auth.BusinessLayer.Models;
using Auth.BusinessLayer.Validators;
using AutoMapper;
using FluentValidation;
using Marvelous.Contracts.ExchangeModels;
using MassTransit;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Auth.BusinessLayer.Test;

public class CrmAddOrChangeLeadConsumerTests : VerifyHelper
{

    #region SetUp

    #pragma warning disable CS8618
    private IMemoryCache _cache;
    private IConsumer<LeadFullExchangeModel> _consumer;
    private Mock<ILogger<CrmAddOrChangeLeadConsumer>> _logger;
    private IMapper _mapper;
    private IValidator<LeadFullExchangeModel> _validator;
    #pragma warning restore CS8618

    [SetUp]
    public void SetUp()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _logger = new Mock<ILogger<CrmAddOrChangeLeadConsumer>>();
        _mapper = new Mapper(new MapperConfiguration(config => config.AddProfile<AutoMapperProfile>()));
        _validator = new LeadFullExchangeModelValidator();

        _consumer = new CrmAddOrChangeLeadConsumer(_logger.Object, _cache, _mapper, _validator);
    }

    #endregion

    #region Consume

    [Test]
    public async Task Consume_ValidRequestReceivedAddOrChange_ShouldApplyChanges()
    {
        //given
        var lead = new LeadFullExchangeModel { Email = "test@example.com", Password = "testPassword", IsBanned = false };
        var context = Mock.Of<ConsumeContext<LeadFullExchangeModel>>(_ => _.Message == lead);

        //when
        await _consumer.Consume(context);

        //then
        Assert.NotNull(_cache.Get<LeadAuthModel>(lead.Email));
        VerifyLogger(_logger, LogLevel.Information, 1);
    }

    [Test]
    public async Task Consume_ValidRequestReceivedRemove_ShouldApplyChanges()
    {
        //given
        var lead = new LeadFullExchangeModel { Email = "test@example.com", Password = "testPassword", IsBanned = true };
        _cache.Set(lead.Email, _mapper.Map<LeadAuthModel>(lead));
        var context = Mock.Of<ConsumeContext<LeadFullExchangeModel>>(_ => _.Message == lead);

        //when
        await _consumer.Consume(context);

        //then
        Assert.IsNull(_cache.Get<LeadAuthModel>(lead.Email).HashPassword);
        Assert.IsTrue(_cache.Get<LeadAuthModel>(lead.Email).Id == default);
        Assert.IsTrue(_cache.Get<LeadAuthModel>(lead.Email).Role == default);
        VerifyLogger(_logger, LogLevel.Information, 1);
    }

    [Test]
    public void Consume_FailedValidate_ShouldThrowValidationException()
    {
        //given
        var lead = new LeadFullExchangeModel();
        var expected =
            "Validation failed: \r\n -- Email: 'Email' должно быть заполнено. Severity: Error\r\n -- Password: 'Password' должно быть заполнено. Severity: Error";
        var context = Mock.Of<ConsumeContext<LeadFullExchangeModel>>(_ => _.Message == lead);

        //when
        var actual = Assert.Throws<ValidationException>(() => _consumer.Consume(context))?.Message;

        //then
        Assert.AreEqual(expected, actual);
        VerifyLogger(_logger, It.IsAny<LogLevel>(), 0);
    }

    #endregion

}