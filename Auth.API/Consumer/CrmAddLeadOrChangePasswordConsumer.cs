using Auth.BusinessLayer.Models;
using AutoMapper;
using Marvelous.Contracts.ExchangeModels;
using MassTransit;
using Microsoft.Extensions.Caching.Memory;

namespace Auth.API.Consumer;

public class CrmAddLeadOrChangePasswordConsumer : IConsumer<LeadFullExchangeModel>
{
    private readonly ILogger<CrmAddLeadOrChangePasswordConsumer> _logger;
    private readonly IMemoryCache _cache;
    private readonly IMapper _mapper;

    public CrmAddLeadOrChangePasswordConsumer(ILogger<CrmAddLeadOrChangePasswordConsumer> logger, IMemoryCache cache, IMapper mapper)
    {
        _logger = logger;
        _cache = cache;
        _mapper = mapper;
    }

    public Task Consume(ConsumeContext<LeadFullExchangeModel> context)
    {
        _logger.LogInformation($"Adding or password change Lead with id = {context.Message.Id}");
        _cache.Set(context.Message.Email, _mapper.Map<LeadAuthModel>(context.Message));
        return Task.CompletedTask;
    }
}