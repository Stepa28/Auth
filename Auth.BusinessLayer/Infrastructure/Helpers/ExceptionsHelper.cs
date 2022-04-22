using Auth.BusinessLayer.Exceptions;
using Auth.BusinessLayer.Models;
using Auth.BusinessLayer.Security;
using Microsoft.Extensions.Logging;
using static System.String;

namespace Auth.BusinessLayer.Helpers;

public class ExceptionsHelper : IExceptionsHelper
{
    private const string NotFound = "Entity with e-mail = {0} not found";
    private const string PasswordIsIncorrected = "Incorrected password";
    private readonly ILogger<ExceptionsHelper> _logger;

    public ExceptionsHelper(ILogger<ExceptionsHelper> logger)
    {
        _logger = logger;
    }

    public void ThrowIfEmailNotFound(string email, LeadAuthModel lead)
    {
        if (!IsNullOrEmpty(lead.HashPassword) || lead.Id != default || lead.Role != default)
            return;

        var ex = new NotFoundException(Format(NotFound, email));
        _logger.LogError(ex, ex.Message);
        throw ex;
    }

    public void ThrowIfPasswordIsIncorrected(string pass, string hashPassFromBd)
    {
        if (PasswordHash.ValidatePassword(pass, hashPassFromBd))
            return;

        var ex = new IncorrectPasswordException(PasswordIsIncorrected);
        _logger.LogError(ex, ex.Message);
        throw ex;
    }
}