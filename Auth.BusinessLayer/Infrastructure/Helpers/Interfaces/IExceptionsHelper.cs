using Auth.BusinessLayer.Models;

namespace Auth.BusinessLayer.Helpers;

public interface IExceptionsHelper
{
    void ThrowIfEmailNotFound(string email, LeadAuthModel lead);
    void ThrowIfPasswordIsIncorrected(string pass, string hashPassFromBd);
}