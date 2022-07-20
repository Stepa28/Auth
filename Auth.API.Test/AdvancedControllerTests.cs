using System.Collections.Generic;
using System.Net;
using System.Security.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;
using Auth.API.Extensions;
using Auth.BusinessLayer.Exceptions;
using Auth.BusinessLayer.Test;
using Auth.Resources;
using Marvelous.Contracts.Enums;
using Marvelous.Contracts.ResponseModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using static Moq.It;

namespace Auth.API.Test;

public class AdvancedControllerTests : VerifyHelper
{

    #region SetUp

    #pragma warning disable CS8618
    private IAdvancedController _advancedController;
    private IMemoryCache _cache;
    private IConfiguration _config;
    private Mock<HttpContext> _context;
    private Controller _controller;
    private Mock<ILogger<AdvancedController>> _logger;
    #pragma warning restore CS8618

    [SetUp]
    public void Setup()
    {
        _config = new ConfigurationBuilder().AddJsonFile("appsettings.Test.json").AddInMemoryCollection(new Dictionary<string, string>()).Build();
        _logger = new Mock<ILogger<AdvancedController>>();
        _cache = new MemoryCache(new MemoryCacheOptions());
        _context = new Mock<HttpContext>();

        _advancedController = new AdvancedController(_logger.Object, _cache, _config, _localizer.Object);
        _controller = new HomeController();
        _advancedController.Controller = _controller;
    }

    #endregion

    #region Service

    [TestCase(Microservice.MarvelousCrm)]
    public void Service_ValidRequestReceived_ShouldMicroservice(Microservice expected)
    {
        //given
        _cache.Set("Initialization task configs", Task.CompletedTask);
        var context = new DefaultHttpContext();
        context.Request.Headers.Add(nameof(Microservice), expected.ToString());
        context.Connection.RemoteIpAddress = IPAddress.Parse(_config["BaseAddress"]);
        _controller.ControllerContext.HttpContext = context;

        //when
        var actual = _advancedController.Service;

        //then
        Assert.AreEqual(expected, actual);
        VerifyLogger(_logger, IsAny<LogLevel>(), 0);
    }

    [Test]
    public void Service_WhenIpNotRegistered_ShouldThrowForbiddenException()
    {
        //given
        _cache.Set("Initialization task configs", Task.CompletedTask);
        _controller.ControllerContext.HttpContext = new DefaultHttpContext { Connection = { RemoteIpAddress = IPAddress.Parse("1") } };

        //when
        Assert.Throws<ForbiddenException>(() =>
        {
            var _ = _advancedController.Service;
        });

        //then
        VerifyLogger(_logger, LogLevel.Error, 1);
    }

    [Test]
    public void Service_WhenNoIndicatorInHeaders_ShouldThrowForbiddenException()
    {
        //given
        _cache.Set("Initialization task configs", Task.CompletedTask);
        _controller.ControllerContext.HttpContext = new DefaultHttpContext { Connection = { RemoteIpAddress = IPAddress.Parse(_config["BaseAddress"]) } };

        //when
        Assert.Throws<ForbiddenException>(() =>
        {
            var _ = _advancedController.Service;
        });

        //then
        VerifyLogger(_logger, LogLevel.Error, 1);
    }

    [Test]
    public void Service_WhenUnregisteredMicroservice_ShouldThrowForbiddenException()
    {
        //given
        _cache.Set("Initialization task configs", Task.CompletedTask);
        var context = new DefaultHttpContext();
        context.Request.Headers.Add(nameof(Microservice), Microservice.MarvelousFrontendUndefined.ToString());
        context.Connection.RemoteIpAddress = IPAddress.Parse(_config["BaseAddress"]);
        _controller.ControllerContext.HttpContext = context;

        //when
        Assert.Throws<ForbiddenException>(() =>
        {
            var _ = _advancedController.Service;
        });

        //then
        VerifyLogger(_logger, LogLevel.Error, 1);
    }

    #endregion

    #region Audience

    [TestCase(Microservice.MarvelousCrm)]
    public void Audience_TokenDecrypted_ShouldAudience(Microservice expected)
    {
        //given
        _context.Setup(s => s.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new("aud", expected.ToString()) })));
        _controller.ControllerContext.HttpContext = _context.Object;

        //when
        var actual = _advancedController.Audience;

        //then
        Assert.AreEqual(expected.ToString(), actual);
        VerifyLogger(_logger, IsAny<LogLevel>(), 0);
    }

    [Test]
    public void Audience_WhenBrokenUser_ShouldThrowAuthenticationException()
    {
        //given
        _context.Setup(s => s.User).Returns(new ClaimsPrincipal());
        _controller.ControllerContext.HttpContext = _context.Object;

        //when
        Assert.Throws<AuthenticationException>(() =>
        {
            var _ = _advancedController.Audience;
        });

        //then
        VerifyLogger(_logger, LogLevel.Error, 1);
    }

    [Test]
    public void Audience_WhenTokenNotExistsAudience_ShouldThrowAuthenticationException()
    {
        //given
        _context.Setup(s => s.User).Returns(new ClaimsPrincipal(new ClaimsIdentity()));
        _controller.ControllerContext.HttpContext = _context.Object;

        //when
        Assert.Throws<AuthenticationException>(() =>
        {
            var _ = _advancedController.Audience;
        });

        //then
        VerifyLogger(_logger, LogLevel.Error, 1);
    }

    #endregion

    #region Issuer

    [TestCase(Microservice.MarvelousCrm)]
    public void Issuer_TokenDecrypted_ShouldIssuer(Microservice expected)
    {
        //given
        _context.Setup(s => s.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new("aud", "", "", expected.ToString()) })));
        _controller.ControllerContext.HttpContext = _context.Object;

        //when
        var actual = _advancedController.Issuer;

        //then
        Assert.AreEqual(expected.ToString(), actual);
        VerifyLogger(_logger, IsAny<LogLevel>(), 0);
    }

    [Test]
    public void Issuer_WhenBrokenUser_ShouldThrowAuthenticationException()
    {
        //given
        _context.Setup(s => s.User).Returns(new ClaimsPrincipal());
        _controller.ControllerContext.HttpContext = _context.Object;

        //when
        Assert.Throws<AuthenticationException>(() =>
        {
            var _ = _advancedController.Issuer;
        });

        //then
        VerifyLogger(_logger, LogLevel.Error, 1);
    }

    [Test]
    public void Issuer_WhenTokenNotExistsAudience_ShouldThrowAuthenticationException()
    {
        //given
        _context.Setup(s => s.User).Returns(new ClaimsPrincipal(new ClaimsIdentity()));
        _controller.ControllerContext.HttpContext = _context.Object;

        //when
        Assert.Throws<AuthenticationException>(() =>
        {
            var _ = _advancedController.Issuer;
        });

        //then
        VerifyLogger(_logger, LogLevel.Error, 1);
    }

    #endregion

    #region MyRegion

    [Test]
    public void Identity_MicroserviceTokenDecrypted_ShouldIdentity()
    {
        //given
        var expected = new IdentityResponseModel { IssuerMicroservice = Microservice.MarvelousCrm.ToString() };
        _context.Setup(s => s.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new("aud", "", "", expected.IssuerMicroservice) })));
        _controller.ControllerContext.HttpContext = _context.Object;

        //when
        var actual = _advancedController.Identity.IssuerMicroservice;

        //then
        Assert.AreEqual(expected.IssuerMicroservice, actual);
        Assert.IsNull(expected.Id);
        Assert.IsNull(expected.Role);
        VerifyLogger(_logger, IsAny<LogLevel>(), 0);
    }

    [Test]
    public void Identity_FrontTokenDecrypted_ShouldIdentity()
    {
        //given
        var expected = new IdentityResponseModel { Id = 1, Role = Role.Admin.ToString(), IssuerMicroservice = Microservice.MarvelousCrm.ToString() };
        _context.Setup(s => s.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new("aud", "", "", expected.IssuerMicroservice),
            new(ClaimTypes.UserData, expected.Id.Value.ToString()),
            new(ClaimTypes.Role, expected.Role)
        })));
        _controller.ControllerContext.HttpContext = _context.Object;

        //when
        var actual = _advancedController.Identity;

        //then
        Assert.AreEqual(expected.IssuerMicroservice, actual.IssuerMicroservice);
        Assert.AreEqual(expected.Id, actual.Id);
        Assert.AreEqual(expected.Role, actual.Role);
        VerifyLogger(_logger, IsAny<LogLevel>(), 0);
    }

    [Test]
    public void Identity_WhenBrokenUser_ShouldThrowAuthenticationException()
    {
        //given
        _context.Setup(s => s.User).Returns(new ClaimsPrincipal());
        _controller.ControllerContext.HttpContext = _context.Object;

        //when
        Assert.Throws<AuthenticationException>(() =>
        {
            var _ = _advancedController.Identity;
        });

        //then
        VerifyLogger(_logger, LogLevel.Error, 1);
    }

    #endregion

}

public class HomeController : Controller {}