using Auth.BusinessLayer.Models;
using AutoMapper;
using FluentValidation;
using Marvelous.Contracts.ExchangeModels;
using MassTransit;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Auth.BusinessLayer.Consumer;

public class CrmAddOrChangeLeadConsumer : IConsumer<LeadFullExchangeModel>
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CrmAddOrChangeLeadConsumer> _logger;
    private readonly IMapper _mapper;
    private readonly IValidator<LeadFullExchangeModel> _validator;

    public CrmAddOrChangeLeadConsumer(ILogger<CrmAddOrChangeLeadConsumer> logger, IMemoryCache cache, IMapper mapper, IValidator<LeadFullExchangeModel> validator)
    {
        _logger = logger;
        _cache = cache;
        _mapper = mapper;
        _validator = validator;
    }

    public Task Consume(ConsumeContext<LeadFullExchangeModel> context)
    {
        _validator.ValidateAndThrow(context.Message);
        if (context.Message.IsBanned)
        {
            _logger.LogInformation($"Remove Lead with id = {context.Message.Id}");
            _cache.Remove(context.Message.Email);
        }
        else
        {
            _logger.LogInformation($"Adding or password change Lead with id = {context.Message.Id}");
            _cache.Set(context.Message.Email, _mapper.Map<LeadAuthModel>(context.Message));
        }

        return Task.CompletedTask;
    }
}