﻿using Auth.BusinessLayer.Exceptions;
using RestSharp;
using System.Net;
using Marvelous.Contracts.Enums;
using RestSharp.Authenticators;

namespace Auth.BusinessLayer.Helpers;

public class RequestHelper : IRequestHelper
{

    public async Task<RestResponse> SendRequest(string url, string path, Method method, string jwtToken, Microservice service)
    {
        var request = new RestRequest(path, method);
        return await GenerateRequest(request, url,jwtToken, service);
    }

    private static async Task<RestResponse> GenerateRequest(RestRequest request, string url, string jwtToken, Microservice service)
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