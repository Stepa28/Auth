using Auth.BusinessLayer.Models;
using Auth.Resources;
using AutoMapper;
using FluentValidation;
using Marvelous.Contracts.ExchangeModels;
using MassTransit;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace Auth.BusinessLayer.Consumer;

public class CrmAddOrChangeLeadConsumer : IConsumer<LeadFullExchangeModel>
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CrmAddOrChangeLeadConsumer> _logger;
    private readonly IMapper _mapper;
    private readonly IValidator<LeadFullExchangeModel> _validator;
    private readonly IStringLocalizer<ExceptionAndLogMessages> _localizer;

    public CrmAddOrChangeLeadConsumer(ILogger<CrmAddOrChangeLeadConsumer> logger, IMemoryCache cache, IMapper mapper, IValidator<LeadFullExchangeModel> validator, IStringLocalizer<ExceptionAndLogMessages> localizer)
    {
        _logger = logger;
        _cache = cache;
        _mapper = mapper;
        _validator = validator;
        _localizer = localizer;
    }

    public Task Consume(ConsumeContext<LeadFullExchangeModel> context)
    {
        _validator.ValidateAndThrow(context.Message);
        if (context.Message.IsBanned)
        {
            _logger.LogInformation(_localizer["RemoveLead", context.Message.Id]);
            _cache.Remove(context.Message.Email);
        }
        else
        {
            _logger.LogInformation(_localizer["AddingPasswordChange", context.Message.Id]);
            _cache.Set(context.Message.Email, _mapper.Map<LeadAuthModel>(context.Message));
        }

        return Task.CompletedTask;
    }
}