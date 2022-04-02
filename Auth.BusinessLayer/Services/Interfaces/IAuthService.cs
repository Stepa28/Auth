using Marvelous.Contracts.Enums;

namespace Auth.BusinessLayer.Services;

public interface IAuthService
{
    Task<string> GetTokenForFront(string email, string pass, Microservice service);
    Task CheckValidTokenAmongMicroservices(string issuerToken, string audienceToken, Microservice service);
    Task CheckValidTokenFront(string issuerToken, string audienceToken, Microservice service);
}