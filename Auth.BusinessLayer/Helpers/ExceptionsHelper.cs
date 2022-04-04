﻿using Auth.BusinessLayer.Exceptions;
using Auth.BusinessLayer.Models;
using Auth.BusinessLayer.Security;
using Microsoft.Extensions.Logging;

namespace Auth.BusinessLayer.Helpers;

public class ExceptionsHelper : IExceptionsHelper
{
    private const string NotFound = "Entity with e-mail = {0} not foun";
    private const string PasswordIsIncorrected = "Incorrected password";
    private readonly ILogger<ExceptionsHelper> _logger;

    public ExceptionsHelper(ILogger<ExceptionsHelper> logger)
    {
        _logger = logger;
    }

    public void ThrowIfEmailNotFound(string email, LeadAuthModel lead)
    {
        if (!string.IsNullOrEmpty(lead.HashPassword) || lead.Id != default || lead.Role != default)
            return;

        _logger.LogError(string.Format(NotFound, email));
        throw new NotFoundException(string.Format(NotFound, email));
    }

    public void ThrowIfPasswordIsIncorrected(string pass, string hashPassFromBd)
    {
        if (PasswordHash.ValidatePassword(pass, hashPassFromBd))
            return;

        _logger.LogError(PasswordIsIncorrected);
        throw new IncorrectPasswordException(PasswordIsIncorrected);
    }
}