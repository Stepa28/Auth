using Auth.BusinessLayer.Models;
using Marvelous.Contracts.ExchangeModels;
using MassTransit;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

namespace Auth.API.Consumer;

public class AccountCheckingChangeRole : IConsumer<List<LeadShortExchangeModel>>
{
    private readonly ILogger<AccountCheckingChangeRole> _logger;
    private readonly IMemoryCache _cache;

    public AccountCheckingChangeRole(ILogger<AccountCheckingChangeRole> logger, IMemoryCache cache)
    {
        _logger = logger;
        _cache = cache;
    }

    public Task Consume(ConsumeContext<List<LeadShortExchangeModel>> context)
    {
        _logger.LogInformation("Change role Leads");
        foreach (var lead in context.Message)
        {
            var tmp = _cache.Get<LeadAuthModel>(lead.Email);
            if (tmp.HashPassword.IsNullOrEmpty())
                continue;
            tmp.Role = lead.Role;
            _cache.Set(lead.Email, tmp);
        }

        return Task.CompletedTask;
    }
}