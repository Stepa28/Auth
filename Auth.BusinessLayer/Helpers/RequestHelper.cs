using Auth.BusinessLayer.Exceptions;
using RestSharp;
using System.Net;
using Marvelous.Contracts.Enums;
using RestSharp.Authenticators;

namespace Auth.BusinessLayer.Helpers;

public class RequestHelper : IRequestHelper
{
    public async Task<RestResponse<T>> SendRequest<T>(string url, string path, Microservice service, string jwtToken)
    {
        var request = new RestRequest(path);
        var client = new RestClient(url);
        client.Authenticator = new JwtAuthenticator(jwtToken);
        client.AddDefaultHeader(nameof(Microservice), Microservice.MarvelousAuth.ToString());
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
            throw new BadGatewayException($"Failed to convert data {response.ErrorException!.Message}");
    }
}