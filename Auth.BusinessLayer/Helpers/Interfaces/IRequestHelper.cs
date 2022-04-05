using Marvelous.Contracts.Enums;
using RestSharp;

namespace Auth.BusinessLayer.Helpers;

public interface IRequestHelper
{
    Task<RestResponse<T>> SendRequestAsync<T>(string url, string path, Method method, Microservice service, string jwtToken);
}