using Marvelous.Contracts.Enums;

namespace Auth.BusinessLayer.Services;

public interface IAuthService
{
    Task<string> GetTokenForFront(string email, string pass, Microservice service);
    Task<string> GetTokenForMicroservice(Microservice service);
    Task CheckValidTokenAmongMicroservices(string issuerToken, string audienceToken, Microservice service);
    Task CheckValidTokenFrontend(string issuerToken, string audienceToken, Microservice service);
    Task<string> GetHashPassword(string password);
}