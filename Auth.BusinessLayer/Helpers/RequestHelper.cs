using Auth.BusinessLayer.Exceptions;
using RestSharp;
using System.Net;
using Marvelous.Contracts.Enums;
using RestSharp.Authenticators;

namespace Auth.BusinessLayer.Helpers;

public class RequestHelper : IRequestHelper
{

    public async Task<RestResponse<T>> SendRequestAsync<T>(string url, string path, Method method, Microservice service, string jwtToken)
    {
        var request = new RestRequest(path, method);
        return await GenerateRequest<T>(request, url, service, jwtToken);
    }

    private static async Task<RestResponse<T>> GenerateRequest<T>(RestRequest request, string url, Microservice service, string jwtToken)
    {
        var client = new RestClient(new RestClientOptions(url)
        {
            Timeout = 300000
        });
        client.Authenticator = new JwtAuthenticator(jwtToken);
        var response = await client.ExecuteAsync<T>(request);
        CheckTransactionError(response, service);
        return response;
    }

    private static void CheckTransactionError<T>(RestResponse<T> response, Microservice service)
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
        if (response.Data is null)
            throw new BadGatewayException($"Content equal's null {response.ErrorException!.Message}");
    }
}