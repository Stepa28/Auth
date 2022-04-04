using Marvelous.Contracts.Enums;
using RestSharp;

namespace Auth.BusinessLayer.Helpers;

public interface IRequestHelper
{
    Task<RestResponse> SendRequestWithTokenAsync(string url, string path, Method method, Microservice service, string jwtToken);
    Task<RestResponse> SendRequestAsync(string url, string path, Method method, Microservice service);
}