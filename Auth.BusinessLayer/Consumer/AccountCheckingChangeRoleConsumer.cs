using Auth.BusinessLayer.Models;
using Marvelous.Contracts.ExchangeModels;
using MassTransit;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Auth.BusinessLayer.Consumer;

public class AccountCheckingChangeRoleConsumer : IConsumer<LeadShortExchangeModel[]>
{
    private readonly ILogger<AccountCheckingChangeRoleConsumer> _logger;
    private readonly IMemoryCache _cache;

    public AccountCheckingChangeRoleConsumer(ILogger<AccountCheckingChangeRoleConsumer> logger, IMemoryCache cache)
    {
        _logger = logger;
        _cache = cache;
    }

    public Task Consume(ConsumeContext<LeadShortExchangeModel[]> context)
    {
        _logger.LogInformation("Change role Leads");
        foreach (var lead in context.Message)
        {
            var tmp = _cache.Get<LeadAuthModel>(lead.Email);
            if (tmp.HashPassword.IsNullOrEmpty() || tmp.Id == 0)
                continue;
            tmp.Role = lead.Role;
            _cache.Set(lead.Email, tmp);
        }

        return Task.CompletedTask;
    }
}