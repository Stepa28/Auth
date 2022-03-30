using Auth.BusinessLayer.Exceptions;
using RestSharp;
using System.Net;

namespace Auth.BusinessLayer.Helpers;

public class RequestHelper : IRequestHelper
{

    public async Task<RestResponse> SendRequest(string url, string path, Method method, MarvelousServices service)
    {
        var request = new RestRequest(path, method);
        return await GenerateRequest(request, url, service);
    }

    private async Task<RestResponse> GenerateRequest(RestRequest request, string url, MarvelousServices service)
    {
        var client = new RestClient(new RestClientOptions(url)
        {
            Timeout = 300000
        });
        var response = await client.ExecuteAsync(request);
        CheckTransactionError(response, service);
        return response;
    }

    private void CheckTransactionError(RestResponse response, MarvelousServices service)
    {
        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                break;
            case HttpStatusCode.RequestTimeout:
                throw new RequestTimeoutException($"Request Timeout {response.ErrorException.Message}");
            case HttpStatusCode.ServiceUnavailable:
                throw new ServiceUnavailableException($"Service Unavailable {response.ErrorException.Message}");
            default:
                throw new BadGatewayException($"Oshibka na storone {service}. {response.ErrorException.Message}");
        }
        if (response.Content == null)
            throw new BadGatewayException($"Content equal's null {response.ErrorException.Message}");
    }
}