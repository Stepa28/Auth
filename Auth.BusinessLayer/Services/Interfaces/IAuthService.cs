using Marvelous.Contracts.Enums;

namespace Auth.BusinessLayer.Services;

public interface IAuthService
{
    string GetTokenForFront(string email, string pass, Microservice service);
    string GetTokenForMicroservice(Microservice service);
    void CheckValidTokenAmongMicroservices(string issuerToken, string audienceToken, Microservice service);
    void CheckValidTokenFrontend(string issuerToken, string audienceToken, Microservice service);
    string GetHashPassword(string password);
}