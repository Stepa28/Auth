using System.Net;
using Auth.BusinessLayer.Exceptions;
using FluentValidation;
using Marvelous.Contracts.Enums;
using RestSharp;
using RestSharp.Authenticators;

namespace Auth.BusinessLayer.Helpers;

public class RequestHelper<T> : IRequestHelper<T> where T : class, new()
{
    private readonly IValidator<T> _validator;
    private readonly IRestClient _client;

    public RequestHelper(IValidator<T> validator, IRestClient client)
    {
        _validator = validator;
        _client = client;
    }

    public async Task<RestResponse<IEnumerable<T>>> SendRequest(string url, Microservice service, string jwtToken)
    {
        var request = new RestRequest(url);
        _client.Authenticator = new JwtAuthenticator(jwtToken);
        _client.AddMicroservice(Microservice.MarvelousAuth);
        var response = await _client.ExecuteAsync<IEnumerable<T>>(request);
        CheckTransactionError(response, service);
        return response;
    }

    private void CheckTransactionError(RestResponse<IEnumerable<T>> response, Microservice service)
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

        foreach (var entity in response.Data)
            _validator.ValidateAndThrow(entity);
    }
}