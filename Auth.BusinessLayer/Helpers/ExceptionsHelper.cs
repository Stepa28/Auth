using Auth.BusinessLayer.Exceptions;
using Auth.BusinessLayer.Models;
using Auth.BusinessLayer.Security;
using NLog;

namespace Auth.BusinessLayer.Helpers;

public static class ExceptionsHelper
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public static void ThrowIfEmailNotFound(string email, LeadAuthModel lead)
    {
        if (lead.HashPassword is null || lead.Id == 0)
        {
            _logger.Error($"Oshibka poiska. Lead c email: {email} ne naiden");
            throw new NotFoundException($"Lead с email: {email} не найден");
        }
    }

    public static void ThrowIfPasswordIsIncorrected(string pass, string hashPassFromBd)
    {
        if (!PasswordHash.ValidatePassword(pass, hashPassFromBd))
        {
            _logger.Error("Oshibka vvoda parolya. Vveden nevernyi parol'.");
            throw new IncorrectPasswordException("Неверный пароль");
        }
    }
}