using Auth.BusinessLayer.Models;
using Auth.Resources;
using FluentValidation;
using Marvelous.Contracts.ExchangeModels;
using MassTransit;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Auth.BusinessLayer.Consumer;

public class AccountCheckingChangeRoleConsumer : IConsumer<LeadShortExchangeModel[]>
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<AccountCheckingChangeRoleConsumer> _logger;
    private readonly IValidator<LeadShortExchangeModel> _validator;
    private readonly IStringLocalizer<ExceptionAndLogMessages> _localizer;

    public AccountCheckingChangeRoleConsumer(ILogger<AccountCheckingChangeRoleConsumer> logger, IMemoryCache cache, IValidator<LeadShortExchangeModel> validator, IStringLocalizer<ExceptionAndLogMessages> localizer)
    {
        _logger = logger;
        _cache = cache;
        _validator = validator;
        _localizer = localizer;
    }

    public Task Consume(ConsumeContext<LeadShortExchangeModel[]> context)
    {
        foreach (var entity in context.Message)
            _validator.ValidateAndThrow(entity);

        _logger.LogInformation(_localizer["ChangeRoleLeads"]);
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