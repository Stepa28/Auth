using Auth.BusinessLayer.Exceptions;
using Auth.BusinessLayer.Models;
using Auth.BusinessLayer.Security;
using Auth.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using static System.String;

namespace Auth.BusinessLayer.Helpers;

public class ExceptionsHelper : IExceptionsHelper
{
    private readonly ILogger<ExceptionsHelper> _logger;
    private readonly IStringLocalizer<ExceptionAndLogMessages> _localizer;

    public ExceptionsHelper(ILogger<ExceptionsHelper> logger, IStringLocalizer<ExceptionAndLogMessages> localizer)
    {
        _logger = logger;
        _localizer = localizer;
    }

    public void ThrowIfEmailNotFound(string email, LeadAuthModel lead)
    {
        if (!IsNullOrEmpty(lead.HashPassword) || lead.Id != default || lead.Role != default)
            return;

        var ex = new NotFoundException(_localizer["EmailNotFound", email]);
        _logger.LogError(ex, ex.Message);
        throw ex;
    }

    public void ThrowIfPasswordIsIncorrected(string pass, string hashPassFromBd)
    {
        if (PasswordHash.ValidatePassword(pass, hashPassFromBd))
            return;

        var ex = new IncorrectPasswordException(_localizer["PasswordIsIncorrected"]);
        _logger.LogError(ex, ex.Message);
        throw ex;
    }
}