using Marvelous.Contracts.Enums;
using RestSharp;

namespace Auth.BusinessLayer.Helpers;

public interface IRequestHelper
{
    Task<RestResponse> SendRequest(string url, string path, Method method, Microservice service);
}