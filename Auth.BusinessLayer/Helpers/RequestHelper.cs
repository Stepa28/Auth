using Auth.BusinessLayer.Exceptions;
using RestSharp;
using System.Net;
using Marvelous.Contracts.Enums;
using RestSharp.Authenticators;

namespace Auth.BusinessLayer.Helpers;

public class RequestHelper : IRequestHelper
{

    public async Task<RestResponse> SendRequestWithTokenAsync(string url, string path, Method method, Microservice service, string jwtToken)
    {
        var request = new RestRequest(path, method);
        return await GenerateRequest(request, url, service, jwtToken);
    }

    public async Task<RestResponse> SendRequestAsync(string url, string path, Method method, Microservice service)
    {
        var request = new RestRequest(path, method);
        return await GenerateRequest(request, url, service);
    }

    private static async Task<RestResponse> GenerateRequest(RestRequest request, string url, Microservice service, string jwtToken = "null")
    {
        var client = new RestClient(new RestClientOptions(url)
        {
            Timeout = 300000
        });
        client.Authenticator = new JwtAuthenticator(jwtToken);
        var response = await client.ExecuteAsync(request);
        CheckTransactionError(response, service);
        return response;
    }

    private static void CheckTransactionError(RestResponse response, Microservice service)
    {
        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                break;
            case HttpStatusCode.RequestTimeout:
                throw new RequestTimeoutException($"Request Timeout {response.ErrorException!.Message}");
            case HttpStatusCode.ServiceUnavailable:
                throw new ServiceUnavailableException($"Service Unavailable {response.ErrorException!.Message}");
            default:
                throw new BadGatewayException($"Error on {service}. {response.ErrorException!.Message}");
        }
        if (response.Content == null)
            throw new BadGatewayException($"Content equal's null {response.ErrorException!.Message}");
    }
}