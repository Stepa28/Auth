using Marvelous.Contracts.Enums;
using RestSharp;

namespace Auth.BusinessLayer.Helpers;

public interface IRequestHelper<T> where T : class, new()
{
    Task<RestResponse<IEnumerable<T>>> SendRequest(string url, string path, Microservice service, string jwtToken);
}