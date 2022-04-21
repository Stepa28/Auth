using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Auth.BusinessLayer.Exceptions;
using Auth.BusinessLayer.Helpers;
using Auth.BusinessLayer.Validators;
using FluentValidation;
using Marvelous.Contracts.Client;
using Marvelous.Contracts.Enums;
using Marvelous.Contracts.ResponseModels;
using Moq;
using NUnit.Framework;
using RestSharp;
using static Moq.It;

namespace Auth.BusinessLayer.Test;

public class RequestHelperTests : VerifyHelper
{

    #region Setup

    #pragma warning disable CS8618
    private IValidator<ConfigResponseModel> _validator;
    private Mock<IRestClient> _client;
    private RequestHelper<ConfigResponseModel> _requestHelper;
    #pragma warning restore CS8618
    private static readonly List<ConfigResponseModel> ListConfigs = new()
    {
        new ConfigResponseModel { Key = "BaseAddress", Value = "80.78.240.4" },
        new ConfigResponseModel { Key = "Address", Value = "::1:4589" }
    };

    private const string Message = "Exceptions test";
    private const Microservice Service = Microservice.MarvelousConfigs;

    [SetUp]
    public void SetUp()
    {
        _client = new Mock<IRestClient>();
        _validator = new ConfigResponseModelValidator();

        _requestHelper = new RequestHelper<ConfigResponseModel>(_validator, _client.Object);
    }

    #endregion

    [Test]
    public async Task SendRequest_ResponseReceived200_ShouldRestResponse()
    {
        //given
        var expected = Mock.Of<RestResponse<IEnumerable<ConfigResponseModel>>>(_ => _.Data == ListConfigs && _.StatusCode == HttpStatusCode.OK);
        _client.Setup(s => s.ExecuteAsync<IEnumerable<ConfigResponseModel>>(IsAny<RestRequest>(), IsAny<CancellationToken>())).ReturnsAsync(expected);

        //when
        var actual = await _requestHelper.SendRequest("", Service, "41651");

        //then
        VerifyRequestHelperTests(_client);
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void SendRequest_WhenResponseReceived408_ShouldThrowRequestTimeoutException()
    {
        //given
        var expected = $"Request Timeout {Message}";
        var response = Mock.Of<RestResponse<IEnumerable<ConfigResponseModel>>>(_ =>
            _.StatusCode == HttpStatusCode.RequestTimeout && _.ErrorException == new Exception(Message));
        _client.Setup(s => s.ExecuteAsync<IEnumerable<ConfigResponseModel>>(IsAny<RestRequest>(), IsAny<CancellationToken>())).ReturnsAsync(response);

        //when
        var actual = Assert.ThrowsAsync<RequestTimeoutException>(() => _requestHelper.SendRequest("", Service, "41651"))!.Message;

        //then
        VerifyRequestHelperTests(_client);
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void SendRequest_WhenResponseReceived503_ShouldThrowServiceUnavailableException()
    {
        //given
        var expected = $"Service Unavailable {Message}";
        var response = Mock.Of<RestResponse<IEnumerable<ConfigResponseModel>>>(_ =>
            _.StatusCode == HttpStatusCode.ServiceUnavailable && _.ErrorException == new Exception(Message));
        _client.Setup(s => s.ExecuteAsync<IEnumerable<ConfigResponseModel>>(IsAny<RestRequest>(), IsAny<CancellationToken>())).ReturnsAsync(response);

        //when
        var actual = Assert.ThrowsAsync<ServiceUnavailableException>(() => _requestHelper.SendRequest("", Service, "41651"))!.Message;

        //then
        VerifyRequestHelperTests(_client);
        Assert.AreEqual(expected, actual);
    }

    [TestCase(HttpStatusCode.Accepted)]
    [TestCase(HttpStatusCode.Moved)]
    [TestCase(HttpStatusCode.EarlyHints)]
    [TestCase(HttpStatusCode.UnavailableForLegalReasons)]
    [TestCase(HttpStatusCode.PreconditionRequired)]
    [TestCase(HttpStatusCode.NotModified)]
    [TestCase(HttpStatusCode.PartialContent)]
    public void SendRequest_WhenResponseReceivedAllTheRest_ShouldThrowBadGatewayException(HttpStatusCode statusCode)
    {
        //given
        var expected = $"Error on {Service}. {Message}";
        var response = Mock.Of<RestResponse<IEnumerable<ConfigResponseModel>>>(_ => _.StatusCode == statusCode && _.ErrorException == new Exception(Message));
        _client.Setup(s => s.ExecuteAsync<IEnumerable<ConfigResponseModel>>(IsAny<RestRequest>(), IsAny<CancellationToken>())).ReturnsAsync(response);

        //when
        var actual = Assert.ThrowsAsync<BadGatewayException>(() => _requestHelper.SendRequest("", Service, "41651"))!.Message;

        //then
        VerifyRequestHelperTests(_client);
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void SendRequest_WhenDataCouldNotBeSerialized_ShouldThrowBadGatewayException()
    {
        //given
        var expected = $"Failed to convert data {Message}";
        var response = Mock.Of<RestResponse<IEnumerable<ConfigResponseModel>>>(_ =>
            _.StatusCode == HttpStatusCode.OK && _.ErrorException == new Exception(Message));
        _client.Setup(s => s.ExecuteAsync<IEnumerable<ConfigResponseModel>>(IsAny<RestRequest>(), IsAny<CancellationToken>())).ReturnsAsync(response);

        //when
        var actual = Assert.ThrowsAsync<BadGatewayException>(() => _requestHelper.SendRequest("", Service, "41651"))!.Message;

        //then
        VerifyRequestHelperTests(_client);
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void SendRequest_WhenValidationFailed_ShouldThrowValidationException()
    {
        //given
        var expected =
            "Validation failed: \r\n -- Key: 'Key' должно быть заполнено. Severity: Error\r\n -- Value: 'Value' должно быть заполнено. Severity: Error";
        var response = Mock.Of<RestResponse<IEnumerable<ConfigResponseModel>>>(_ =>
            _.StatusCode == HttpStatusCode.OK && _.Data == new ConfigResponseModel[] { new() });
        _client.Setup(s => s.ExecuteAsync<IEnumerable<ConfigResponseModel>>(IsAny<RestRequest>(), IsAny<CancellationToken>())).ReturnsAsync(response);

        //when
        var actual = Assert.ThrowsAsync<ValidationException>(() => _requestHelper.SendRequest("", Service, "41651"))!.Message;

        //then
        VerifyRequestHelperTests(_client);
        Assert.AreEqual(expected, actual);
    }
}