using System;
using System.IO;
using System.Security.Authentication;
using System.Threading.Tasks;
using Auth.API.Infrastructure;
using Marvelous.Contracts.ResponseModels;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Auth.BusinessLayer.Exceptions;
using FluentValidation;
using NUnit.Framework;

namespace Auth.API.Test;

public class ErrorExceptionMiddlewareTests
{
    #pragma warning disable CS8618
    private DefaultHttpContext _defaultContext;
    #pragma warning restore CS8618
    private const string ExceptionMassage = "Exception massage";

    [SetUp]
    public void SetUp()
    {
        _defaultContext = new DefaultHttpContext
        {
            Response = { Body = new MemoryStream() },
            Request = { Path = "/" }
        };
    }

    [Test]
    public void Invoke_ValidRequestReceived_ShouldResponse()
    {
        //given
        const string expectedOutput = "Request handed over to next request delegate";
        var middlewareInstance = new ErrorExceptionMiddleware(innerHttpContext =>
        {
            innerHttpContext.Response.WriteAsync(expectedOutput);
            return Task.CompletedTask;
        });

        //when
        middlewareInstance.Invoke(_defaultContext);

        //then
        var actual = GetResponseBody();
        Assert.AreEqual(expectedOutput, actual);
    }

    [Test]
    public void Invoke_WhenThrowAuthenticationException_ShouldExceptionResponseModel()
    {
        //given
        var expected = GetJsonExceptionResponseModel(401);
        var middlewareInstance = new ErrorExceptionMiddleware(_ => throw new AuthenticationException(ExceptionMassage));

        //when
        middlewareInstance.Invoke(_defaultContext);

        //then
        var actual = GetResponseBody();
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void Invoke_WhenThrowForbiddenException_ShouldExceptionResponseModel()
    {
        //given
        var expected = GetJsonExceptionResponseModel(403);
        var middlewareInstance = new ErrorExceptionMiddleware(_ => throw new ForbiddenException(ExceptionMassage));

        //when
        middlewareInstance.Invoke(_defaultContext);

        //then
        var actual = GetResponseBody();
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void Invoke_WhenThrowNotFoundException_ShouldExceptionResponseModel()
    {
        //given
        var expected = GetJsonExceptionResponseModel(404);
        var middlewareInstance = new ErrorExceptionMiddleware(_ => throw new NotFoundException(ExceptionMassage));

        //when
        middlewareInstance.Invoke(_defaultContext);

        //then
        var actual = GetResponseBody();
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void Invoke_WhenThrowBadRequestException_ShouldExceptionResponseModel()
    {
        //given
        var expected = GetJsonExceptionResponseModel(400);
        var middlewareInstance = new ErrorExceptionMiddleware(_ => throw new BadRequestException(ExceptionMassage));

        //when
        middlewareInstance.Invoke(_defaultContext);

        //then
        var actual = GetResponseBody();
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void Invoke_WhenThrowServiceUnavailableException_ShouldExceptionResponseModel()
    {
        //given
        var expected = GetJsonExceptionResponseModel(503);
        var middlewareInstance = new ErrorExceptionMiddleware(_ => throw new ServiceUnavailableException(ExceptionMassage));

        //when
        middlewareInstance.Invoke(_defaultContext);

        //then
        var actual = GetResponseBody();
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void Invoke_WhenThrowValidationException_ShouldExceptionResponseModel()
    {
        //given
        var expected = GetJsonExceptionResponseModel(422);
        var middlewareInstance = new ErrorExceptionMiddleware(_ => throw new ValidationException(ExceptionMassage));

        //when
        middlewareInstance.Invoke(_defaultContext);

        //then
        var actual = GetResponseBody();
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void Invoke_WhenThrowIncorrectPasswordException_ShouldExceptionResponseModel()
    {
        //given
        var expected = GetJsonExceptionResponseModel(409);
        var middlewareInstance = new ErrorExceptionMiddleware(_ => throw new IncorrectPasswordException(ExceptionMassage));

        //when
        middlewareInstance.Invoke(_defaultContext);

        //then
        var actual = GetResponseBody();
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void Invoke_WhenThrowException_ShouldExceptionResponseModel()
    {
        //given
        var expected = GetJsonExceptionResponseModel(400);
        var middlewareInstance = new ErrorExceptionMiddleware(_ => throw new Exception(ExceptionMassage));

        //when
        middlewareInstance.Invoke(_defaultContext);

        //then
        var actual = GetResponseBody();
        Assert.AreEqual(expected, actual);
    }

    private static string GetJsonExceptionResponseModel(int statusCode) =>
        JsonSerializer.Serialize(new ExceptionResponseModel { Code = statusCode, Message = ExceptionMassage });

    private string GetResponseBody()
    {
        _defaultContext.Response.Body.Seek(0, SeekOrigin.Begin);
        return new StreamReader(_defaultContext.Response.Body).ReadToEnd();
    }
}