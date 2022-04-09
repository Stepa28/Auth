using Marvelous.Contracts.Enums;
using RestSharp;

namespace Auth.BusinessLayer.Helpers;

public interface IRequestHelper
{
    Task<RestResponse<T>> SendRequest<T>(string url, string path, Microservice service, string jwtToken);
}