using System.Security.Authentication;
using System.Threading.Tasks;
using Auth.API.Controllers;
using Auth.API.Extensions;
using Auth.BusinessLayer.Exceptions;
using Auth.BusinessLayer.Services;
using Auth.BusinessLayer.Test;
using Auth.BusinessLayer.Validators;
using FluentValidation;
using Marvelous.Contracts.Enums;
using Marvelous.Contracts.RequestModels;
using Marvelous.Contracts.ResponseModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using static Moq.It;

namespace Auth.API.Test;

public class AuthorizationsControllerTests : VerifyHelper
{

    #region SetUp

    #pragma warning disable CS8618
    private AuthorizationsController _controller;
    private Mock<IAdvancedController> _advancedController;
    private Mock<IAuthService> _authService;
    private IMemoryCache _cache;
    private Mock<ILogger<AuthorizationsController>> _logger;
    private IValidator<AuthRequestModel> _validator;
    #pragma warning restore CS8618
    private const Microservice Crm = Microservice.MarvelousCrm;

    [SetUp]
    public void Setup()
    {
        _logger = new Mock<ILogger<AuthorizationsController>>();
        _cache = new MemoryCache(new MemoryCacheOptions());
        _advancedController = new Mock<IAdvancedController>();
        _authService = new Mock<IAuthService>();
        _validator = new AuthRequestModelValidator();

        _controller = new AuthorizationsController(_authService.Object, _logger.Object, _cache, _validator, _advancedController.Object, _localizer.Object);
    }

    #endregion

    #region Login

    [Test]
    public void Login_ValidRequestReceived_Should200OkToken()
    {
        //given
        _cache.Set("Initialization task lead", Task.CompletedTask);
        var auth = new AuthRequestModel { Email = "test@example.com", Password = "testPassword" };
        var expected = "Test token";
        _advancedController.Setup(s => s.Service).Returns(Crm);
        _authService.Setup(s => s.GetTokenForFront(IsAny<string>(), IsAny<string>(), IsAny<Microservice>())).Returns(expected);

        //when
        var actual = _controller.Login(auth).Result as OkObjectResult;

        //then
        _authService.Verify(v => v.GetTokenForFront(auth.Email, auth.Password, Crm), Times.Once);
        Assert.AreEqual(StatusCodes.Status200OK, actual!.StatusCode);
        Assert.AreEqual(expected, actual.Value);
        VerifyAdvancedController(_advancedController, 1, 0, 0, 0);
        VerifyLogger(_logger, LogLevel.Information, 2);
    }

    [Test]
    public void Login_WhenAdvancedControllerThrowForbiddenException_ShouldThrowingException()
    {
        //given
        _cache.Set("Initialization task lead", Task.CompletedTask);
        var auth = new AuthRequestModel { Email = "test@example.com", Password = "testPassword" };
        var expected = "Extension massage";
        _advancedController.Setup(s => s.Service).Throws(new ForbiddenException(expected));

        //when
        var actual = Assert.Throws<ForbiddenException>(() => _controller.Login(auth))!.Message;

        //then
        Assert.AreEqual(expected, actual);
        VerifyAdvancedController(_advancedController, 1, 0, 0, 0);
        VerifyLogger(_logger, LogLevel.Information, 1);
    }

    [Test]
    public void Login_WhenAuthServiceThrowNotFoundException_ShouldThrowingException()
    {
        //given
        _cache.Set("Initialization task lead", Task.CompletedTask);
        var auth = new AuthRequestModel { Email = "test@example.com", Password = "testPassword" };
        var expected = "Extension massage";
        _advancedController.Setup(s => s.Service).Returns(Crm);
        _authService.Setup(s => s.GetTokenForFront(IsAny<string>(), IsAny<string>(), IsAny<Microservice>())).Throws(new NotFoundException(expected));

        //when
        var actual = Assert.Throws<NotFoundException>(() => _controller.Login(auth))!.Message;

        //then
        _authService.Verify(v => v.GetTokenForFront(auth.Email, auth.Password, Crm), Times.Once);
        Assert.AreEqual(expected, actual);
        VerifyAdvancedController(_advancedController, 1, 0, 0, 0);
        VerifyLogger(_logger, LogLevel.Information, 1);
    }

    [Test]
    public void Login_WhenAuthServiceThrowIncorrectPasswordException_ShouldThrowingException()
    {
        //given
        _cache.Set("Initialization task lead", Task.CompletedTask);
        var auth = new AuthRequestModel { Email = "test@example.com", Password = "testPassword" };
        var expected = "Extension massage";
        _advancedController.Setup(s => s.Service).Returns(Crm);
        _authService.Setup(s => s.GetTokenForFront(IsAny<string>(), IsAny<string>(), IsAny<Microservice>())).Throws(new IncorrectPasswordException(expected));

        //when
        var actual = Assert.Throws<IncorrectPasswordException>(() => _controller.Login(auth))!.Message;

        //then
        _authService.Verify(v => v.GetTokenForFront(auth.Email, auth.Password, Crm), Times.Once);
        Assert.AreEqual(expected, actual);
        VerifyAdvancedController(_advancedController, 1, 0, 0, 0);
        VerifyLogger(_logger, LogLevel.Information, 1);
    }

    [Test]
    public void Login_WhenAuthServiceThrowServiceUnavailableException_ShouldThrowingException()
    {
        //given
        _cache.Set("Initialization task lead", Task.CompletedTask);
        var auth = new AuthRequestModel { Email = "test@example.com", Password = "testPassword" };
        var expected = "Extension massage";
        _advancedController.Setup(s => s.Service).Returns(Crm);
        _authService.Setup(s => s.GetTokenForFront(IsAny<string>(), IsAny<string>(), IsAny<Microservice>())).Throws(new ServiceUnavailableException(expected));

        //when
        var actual = Assert.Throws<ServiceUnavailableException>(() => _controller.Login(auth))!.Message;

        //then
        _authService.Verify(v => v.GetTokenForFront(auth.Email, auth.Password, Crm), Times.Once);
        Assert.AreEqual(expected, actual);
        VerifyAdvancedController(_advancedController, 1, 0, 0, 0);
        VerifyLogger(_logger, LogLevel.Information, 1);
    }

    [Test]
    public void Login_WhenAuthModelIsNull_ShouldThrowBadRequestException()
    {
        //given when
        Assert.Throws<BadRequestException>(() => _controller.Login(null));

        //then
        VerifyAdvancedController(_advancedController, 0, 0, 0, 0);
        VerifyLogger(_logger, LogLevel.Error, 1);
    }

    [Test]
    public void Login_WhenAuthModelIsEmpty_ShouldThrowValidationException()
    {
        //given
        var expected =
            "Validation failed: \r\n -- Email: 'Email' должно быть заполнено. Severity: Error\r\n -- Password: 'Password' должно быть заполнено. Severity: Error";

        //when
        var actual = Assert.Throws<ValidationException>(() => _controller.Login(new AuthRequestModel()))!.Message;

        //then
        Assert.AreEqual(expected, actual);
        VerifyAdvancedController(_advancedController, 0, 0, 0, 0);
        VerifyLogger(_logger, LogLevel.Error, 1);
    }

    #endregion

    #region GetTokenForMicroservice

    [Test]
    public void GetTokenForMicroservice_ValidRequestReceived_Should200OkToken()
    {
        //given
        var expected = "Test token";
        _advancedController.Setup(s => s.Service).Returns(Crm);
        _authService.Setup(s => s.GetTokenForMicroservice(IsAny<Microservice>())).Returns(expected);

        //when
        var actual = _controller.GetTokenForMicroservice().Result as OkObjectResult;

        //then
        _authService.Verify(v => v.GetTokenForMicroservice(Crm), Times.Once);
        Assert.AreEqual(StatusCodes.Status200OK, actual!.StatusCode);
        Assert.AreEqual(expected, actual.Value);
        VerifyAdvancedController(_advancedController, 1, 0, 0, 0);
        VerifyLogger(_logger, LogLevel.Information, 1);
    }

    [Test]
    public void GetTokenForMicroservice_WhenAdvancedControllerThrowForbiddenException_ShouldThrowingException()
    {
        //given
        var expected = "Extension massage";
        _advancedController.Setup(s => s.Service).Throws(new ForbiddenException(expected));

        //when
        var actual = Assert.Throws<ForbiddenException>(() => _controller.GetTokenForMicroservice())!.Message;

        //then
        Assert.AreEqual(expected, actual);
        VerifyAdvancedController(_advancedController, 1, 0, 0, 0);
        VerifyLogger(_logger, IsAny<LogLevel>(), 0);
    }

    #endregion

    #region CheckTokenAmongMicroservices

    [TestCase(Microservice.MarvelousTransactionStore, "MarvelousCrm", "MarvelousTransactionStore")]
    public void CheckTokenAmongMicroservices_IssuerMicroserviceNotAuth_Should200OkIdentity(Microservice service, string issuer, string audience)
    {
        //given
        var expected = new IdentityResponseModel { IssuerMicroservice = Microservice.MarvelousCrm.ToString() };
        _advancedController.Setup(s => s.Service).Returns(service);
        _advancedController.Setup(s => s.Issuer).Returns(issuer);
        _advancedController.Setup(s => s.Audience).Returns(audience);
        _authService.Setup(s => s.CheckValidTokenAmongMicroservices(IsAny<string>(), IsAny<string>(), IsAny<Microservice>()));
        _advancedController.Setup(s => s.Identity).Returns(expected);

        //when
        var actual = _controller.CheckTokenAmongMicroservices().Result as OkObjectResult;

        //then
        _authService.Verify(v => v.CheckValidTokenAmongMicroservices(issuer, audience, service), Times.Once);
        Assert.AreEqual(StatusCodes.Status200OK, actual!.StatusCode);
        Assert.AreEqual(expected, actual.Value);
        VerifyAdvancedController(_advancedController, 1, 2, 1, 1);
        VerifyLogger(_logger, IsAny<LogLevel>(), 0);
    }

    [TestCase("MarvelousAuth")]
    public void CheckTokenAmongMicroservices_IssuerMicroserviceAuth_Should200OkIdentity(string issuer)
    {
        //given
        var expected = new IdentityResponseModel { IssuerMicroservice = Microservice.MarvelousAuth.ToString() };
        _advancedController.Setup(s => s.Issuer).Returns(issuer);
        _advancedController.Setup(s => s.Identity).Returns(expected);

        //when
        var actual = _controller.CheckTokenAmongMicroservices().Result as OkObjectResult;

        //then
        Assert.AreEqual(StatusCodes.Status200OK, actual!.StatusCode);
        Assert.AreEqual(expected, actual.Value);
        VerifyAdvancedController(_advancedController, 0, 1, 0, 1);
        VerifyLogger(_logger, LogLevel.Information, 1);
    }

    [TestCase("MarvelousCrm")]
    public void CheckTokenAmongMicroservices_WhenAdvancedControllerThrowForbiddenException_ShouldThrowingException(string issuer)
    {
        //given
        var expected = "Extension massage";
        _advancedController.Setup(s => s.Issuer).Returns(issuer);
        _advancedController.Setup(s => s.Service).Throws(new ForbiddenException(expected));

        //when
        var actual = Assert.Throws<ForbiddenException>(() => _controller.CheckTokenAmongMicroservices())!.Message;

        //then
        Assert.AreEqual(expected, actual);
        VerifyAdvancedController(_advancedController, 1, 2, 1, 0);
        VerifyLogger(_logger, IsAny<LogLevel>(), 0);
    }

    [TestCase(Microservice.MarvelousCrm, Microservice.MarvelousFrontendCrm)]
    public void CheckTokenAmongMicroservices_WhenAuthServiceThrowForbiddenException_ShouldThrowingException(Microservice service, Microservice audience)
    {
        //given
        var expected = "Extension massage";
        SetupAdvancedController(_advancedController, service, audience);
        _authService.Setup(s => s.CheckValidTokenAmongMicroservices(IsAny<string>(), IsAny<string>(), IsAny<Microservice>()))
                    .Throws(new ForbiddenException(expected));

        //when
        var actual = Assert.Throws<ForbiddenException>(() => _controller.CheckTokenAmongMicroservices())!.Message;

        //then
        _authService.Verify(v => v.CheckValidTokenAmongMicroservices(service.ToString(), audience.ToString(), service), Times.Once);
        Assert.AreEqual(expected, actual);
        VerifyAdvancedController(_advancedController, 1, 2, 1, 0);
        VerifyLogger(_logger, IsAny<LogLevel>(), 0);
    }

    [Test]
    public void CheckTokenAmongMicroservices_WhenAdvancedControllerThrowAuthenticationException_ShouldThrowingException()
    {
        //given
        var expected = "Extension massage";
        _advancedController.Setup(s => s.Issuer).Throws(new AuthenticationException(expected));

        //when
        var actual = Assert.Throws<AuthenticationException>(() => _controller.CheckTokenAmongMicroservices())!.Message;

        //then
        Assert.AreEqual(expected, actual);
        VerifyAdvancedController(_advancedController, 0, 1, 0, 0);
        VerifyLogger(_logger, IsAny<LogLevel>(), 0);
    }

    [TestCase(Microservice.MarvelousCrm, Microservice.MarvelousFrontendCrm)]
    public void CheckTokenAmongMicroservices_WhenAuthServiceThrowAuthenticationException_ShouldThrowingException(Microservice service, Microservice audience)
    {
        //given
        var expected = "Extension massage";
        SetupAdvancedController(_advancedController, service, audience);
        _authService.Setup(s => s.CheckValidTokenAmongMicroservices(IsAny<string>(), IsAny<string>(), IsAny<Microservice>()))
                    .Throws(new AuthenticationException(expected));

        //when
        var actual = Assert.Throws<AuthenticationException>(() => _controller.CheckTokenAmongMicroservices())!.Message;

        //then
        _authService.Verify(v => v.CheckValidTokenAmongMicroservices(service.ToString(), audience.ToString(), service), Times.Once);
        Assert.AreEqual(expected, actual);
        VerifyAdvancedController(_advancedController, 1, 2, 1, 0);
        VerifyLogger(_logger, IsAny<LogLevel>(), 0);
    }

    #endregion

    #region CheckTokenFrontend

    [TestCase(Microservice.MarvelousCrm, Microservice.MarvelousFrontendCrm)]
    public void CheckTokenFrontend_ValidRequestReceived_Should200OkIdentity(Microservice service, Microservice audience)
    {
        //given
        var expected = new IdentityResponseModel { Id = 2, Role = Role.Admin.ToString(), IssuerMicroservice = Microservice.MarvelousCrm.ToString() };
        SetupAdvancedController(_advancedController, service, audience);
        _authService.Setup(s => s.CheckValidTokenFrontend(IsAny<string>(), IsAny<string>(), IsAny<Microservice>()));
        _advancedController.Setup(s => s.Identity).Returns(expected);

        //when
        var actual = _controller.CheckTokenFrontend().Result as OkObjectResult;

        //then
        _authService.Verify(v => v.CheckValidTokenFrontend(service.ToString(), audience.ToString(), service), Times.Once);
        Assert.AreEqual(StatusCodes.Status200OK, actual!.StatusCode);
        Assert.AreEqual(expected, actual.Value);
        VerifyAdvancedController(_advancedController, 1, 1, 1, 1);
        VerifyLogger(_logger, IsAny<LogLevel>(), 0);
    }

    [Test]
    public void CheckTokenFrontend_WhenAdvancedControllerThrowForbiddenException_ShouldThrowingException()
    {
        //given
        var expected = "Extension massage";
        _advancedController.Setup(s => s.Service).Throws(new ForbiddenException(expected));

        //when
        var actual = Assert.Throws<ForbiddenException>(() => _controller.CheckTokenFrontend())!.Message;

        //then
        _authService.Verify(v => v.CheckValidTokenFrontend(IsAny<string>(), IsAny<string>(), IsAny<Microservice>()), Times.Never);
        Assert.AreEqual(expected, actual);
        VerifyAdvancedController(_advancedController, 1, 1, 1, 0);
        VerifyLogger(_logger, IsAny<LogLevel>(), 0);
    }

    [TestCase(Microservice.MarvelousCrm, Microservice.MarvelousFrontendCrm)]
    public void CheckTokenFrontend_WhenAuthServiceThrowForbiddenException_ShouldThrowingException(Microservice service, Microservice audience)
    {
        //given
        var expected = "Extension massage";
        SetupAdvancedController(_advancedController, service, audience);
        _authService.Setup(s => s.CheckValidTokenFrontend(IsAny<string>(), IsAny<string>(), IsAny<Microservice>())).Throws(new ForbiddenException(expected));

        //when
        var actual = Assert.Throws<ForbiddenException>(() => _controller.CheckTokenFrontend())!.Message;

        //then
        _authService.Verify(v => v.CheckValidTokenFrontend(service.ToString(), audience.ToString(), service), Times.Once);
        Assert.AreEqual(expected, actual);
        VerifyAdvancedController(_advancedController, 1, 1, 1, 0);
        VerifyLogger(_logger, IsAny<LogLevel>(), 0);
    }

    [Test]
    public void CheckTokenFrontend_WhenAdvancedControllerThrowAuthenticationException_ShouldThrowingException()
    {
        //given
        var expected = "Extension massage";
        _advancedController.Setup(s => s.Issuer).Throws(new AuthenticationException(expected));

        //when
        var actual = Assert.Throws<AuthenticationException>(() => _controller.CheckTokenFrontend())!.Message;

        //then
        _authService.Verify(v => v.CheckValidTokenFrontend(IsAny<string>(), IsAny<string>(), IsAny<Microservice>()), Times.Never);
        Assert.AreEqual(expected, actual);
        VerifyAdvancedController(_advancedController, 0, 1, 0, 0);
        VerifyLogger(_logger, IsAny<LogLevel>(), 0);
    }

    [TestCase(Microservice.MarvelousCrm, Microservice.MarvelousFrontendCrm)]
    public void CheckTokenFrontend_WhenAuthServiceThrowAuthenticationException_ShouldThrowingException(Microservice service, Microservice audience)
    {
        //given
        var expected = "Extension massage";
        SetupAdvancedController(_advancedController, service, audience);
        _authService.Setup(s => s.CheckValidTokenFrontend(IsAny<string>(), IsAny<string>(), IsAny<Microservice>())).Throws(new AuthenticationException(expected));

        //when
        var actual = Assert.Throws<AuthenticationException>(() => _controller.CheckTokenFrontend())!.Message;

        //then
        _authService.Verify(v => v.CheckValidTokenFrontend(service.ToString(), audience.ToString(), service), Times.Once);
        Assert.AreEqual(expected, actual);
        VerifyAdvancedController(_advancedController, 1, 1, 1, 0);
        VerifyLogger(_logger, IsAny<LogLevel>(), 0);
    }

    [TestCase(Microservice.MarvelousCrm, Microservice.MarvelousFrontendCrm)]
    public void CheckTokenFrontend_WhenIdentityNotExistsId_ShouldThrowForbiddenException(Microservice service, Microservice audience)
    {
        //given
        var identity = new IdentityResponseModel { IssuerMicroservice = Microservice.MarvelousCrm.ToString() };
        SetupAdvancedController(_advancedController, service, audience);
        _authService.Setup(s => s.CheckValidTokenFrontend(IsAny<string>(), IsAny<string>(), IsAny<Microservice>()));
        _advancedController.Setup(s => s.Identity).Returns(identity);

        //when
        Assert.Throws<ForbiddenException>(() => _controller.CheckTokenFrontend());

        //then
        _authService.Verify(v => v.CheckValidTokenFrontend(service.ToString(), audience.ToString(), service), Times.Once);
        VerifyAdvancedController(_advancedController, 2, 1, 1, 1);
        VerifyLogger(_logger, IsAny<LogLevel>(), 0);
    }

    #endregion

    #region DoubleCheckToken

    [TestCase(Microservice.MarvelousCrm, Microservice.MarvelousFrontendCrm)]
    public void DoubleCheckToken_ValidRequestReceived_Should200OkIdentity(Microservice service, Microservice audience)
    {
        //given
        var expected = new IdentityResponseModel { Id = 2, Role = Role.Admin.ToString(), IssuerMicroservice = Microservice.MarvelousCrm.ToString() };
        SetupAdvancedController(_advancedController, service, audience);
        _authService.Setup(s => s.CheckDoubleValidToken(IsAny<string>(), IsAny<string>(), IsAny<Microservice>()));
        _advancedController.Setup(s => s.Identity).Returns(expected);

        //when
        var actual = _controller.DoubleCheckToken().Result as OkObjectResult;

        //then
        _authService.Verify(v => v.CheckDoubleValidToken(service.ToString(), audience.ToString(), service), Times.Once);
        Assert.AreEqual(StatusCodes.Status200OK, actual!.StatusCode);
        Assert.AreEqual(expected, actual.Value);
        VerifyAdvancedController(_advancedController, 1, 1, 1, 1);
        VerifyLogger(_logger, IsAny<LogLevel>(), 0);
    }

    [Test]
    public void CheckDoubleValidToken_WhenAdvancedControllerThrowForbiddenException_ShouldThrowingException()
    {
        //given
        var expected = "Extension massage";
        _advancedController.Setup(s => s.Service).Throws(new ForbiddenException(expected));

        //when
        var actual = Assert.Throws<ForbiddenException>(() => _controller.DoubleCheckToken())!.Message;

        //then
        _authService.Verify(v => v.CheckDoubleValidToken(IsAny<string>(), IsAny<string>(), IsAny<Microservice>()), Times.Never);
        Assert.AreEqual(expected, actual);
        VerifyAdvancedController(_advancedController, 1, 1, 1, 0);
        VerifyLogger(_logger, IsAny<LogLevel>(), 0);
    }

    [TestCase(Microservice.MarvelousCrm, Microservice.MarvelousFrontendCrm)]
    public void CheckDoubleValidToken_WhenAuthServiceThrowForbiddenException_ShouldThrowingException(Microservice service, Microservice audience)
    {
        //given
        var expected = "Extension massage";
        SetupAdvancedController(_advancedController, service, audience);
        _authService.Setup(s => s.CheckDoubleValidToken(IsAny<string>(), IsAny<string>(), IsAny<Microservice>())).Throws(new ForbiddenException(expected));

        //when
        var actual = Assert.Throws<ForbiddenException>(() => _controller.DoubleCheckToken())!.Message;

        //then
        _authService.Verify(v => v.CheckDoubleValidToken(service.ToString(), audience.ToString(), service), Times.Once);
        Assert.AreEqual(expected, actual);
        VerifyAdvancedController(_advancedController, 1, 1, 1, 0);
        VerifyLogger(_logger, IsAny<LogLevel>(), 0);
    }

    [Test]
    public void CheckDoubleValidToken_WhenAdvancedControllerThrowAuthenticationException_ShouldThrowingException()
    {
        //given
        var expected = "Extension massage";
        _advancedController.Setup(s => s.Issuer).Throws(new AuthenticationException(expected));


        //when
        var actual = Assert.Throws<AuthenticationException>(() => _controller.DoubleCheckToken())!.Message;

        //then
        _authService.Verify(v => v.CheckValidTokenFrontend(IsAny<string>(), IsAny<string>(), IsAny<Microservice>()), Times.Never);
        Assert.AreEqual(expected, actual);
        VerifyAdvancedController(_advancedController, 0, 1, 0, 0);
        VerifyLogger(_logger, IsAny<LogLevel>(), 0);
    }

    [TestCase(Microservice.MarvelousCrm, Microservice.MarvelousFrontendCrm)]
    public void CheckDoubleValidToken_WhenAuthServiceThrowAuthenticationException_ShouldThrowingException(Microservice service, Microservice audience)
    {
        //given
        var expected = "Extension massage";
        SetupAdvancedController(_advancedController, service, audience);
        _authService.Setup(s => s.CheckDoubleValidToken(IsAny<string>(), IsAny<string>(), IsAny<Microservice>())).Throws(new AuthenticationException(expected));

        //when
        var actual = Assert.Throws<AuthenticationException>(() => _controller.DoubleCheckToken())!.Message;

        //then
        _authService.Verify(v => v.CheckDoubleValidToken(service.ToString(), audience.ToString(), service), Times.Once);
        Assert.AreEqual(expected, actual);
        VerifyAdvancedController(_advancedController, 1, 1, 1, 0);
        VerifyLogger(_logger, IsAny<LogLevel>(), 0);
    }

    #endregion

    #region GetHashingString

    [TestCase(Microservice.MarvelousCrm)]
    public void GetHashingString_ValidRequestReceived_Should200OkHash(Microservice service)
    {
        //given
        var expected = "1000:agaohguiuoiah9pa:fajhsoupidhjgafp";
        var password = "password";
        _advancedController.Setup(s => s.Service).Returns(service);
        _authService.Setup(s => s.GetHashPassword(IsAny<string>())).Returns(expected);

        //when
        var actual = _controller.GetHashingString(password).Result as OkObjectResult;

        //then
        _authService.Verify(v => v.GetHashPassword(password), Times.Once);
        Assert.AreEqual(StatusCodes.Status200OK, actual!.StatusCode);
        Assert.AreEqual(expected, actual.Value);
        VerifyAdvancedController(_advancedController, 1, 0, 0, 0);
        VerifyLogger(_logger, LogLevel.Information, 2);
    }

    [Test]
    public void GetHashingString_WhenAdvancedControllerThrowForbiddenException_ShouldThrowingException()
    {
        //given
        var expected = "Extension massage";
        var password = "password";
        _advancedController.Setup(s => s.Service).Throws(new ForbiddenException(expected));

        //when
        var actual = Assert.Throws<ForbiddenException>(() => _controller.GetHashingString(password))!.Message;

        //then
        Assert.AreEqual(expected, actual);
        VerifyAdvancedController(_advancedController, 1, 0, 0, 0);
        VerifyLogger(_logger, IsAny<LogLevel>(), 0);
    }

    [TestCase(Microservice.MarvelousCrm)]
    public void GetHashingString_WhenPasswordIsNullOrEmpty_ShouldThrowBadRequestException(Microservice service)
    {
        //given
        _advancedController.Setup(s => s.Service).Returns(service);

        //when
        Assert.Throws<BadRequestException>(() => _controller.GetHashingString(null));

        //then
        VerifyAdvancedController(_advancedController, 1, 0, 0, 0);
        VerifyLogger(_logger, LogLevel.Information, 1);
        VerifyLogger(_logger, LogLevel.Error, 1);
    }

    #endregion

}