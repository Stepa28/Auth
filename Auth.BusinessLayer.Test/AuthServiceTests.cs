using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using Auth.BusinessLayer.Exceptions;
using Auth.BusinessLayer.Helpers;
using Auth.BusinessLayer.Models;
using Auth.BusinessLayer.Security;
using Auth.BusinessLayer.Services;
using Auth.BusinessLayer.Test.TestCaseSources;
using Marvelous.Contracts.Enums;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NUnit.Framework;
using static Moq.It;

namespace Auth.BusinessLayer.Test;

public class AuthServiceTests : VerifyHelper
{

    #region SetUp

    #pragma warning disable CS8618
    private const string SecretKey = "SuperSecretKeyForTest";
    private IAuthService _authService;
    private IMemoryCache _cache;
    private Mock<IConfiguration> _config;
    private Mock<IExceptionsHelper> _exceptionsHelper;
    private Mock<ILogger<AuthService>> _logger;
    private Dictionary<Microservice, MicroserviceModel> _microservices;
    #pragma warning restore CS8618

    [SetUp]
    public void SetUp()
    {
        _logger = new Mock<ILogger<AuthService>>();
        _cache = new MemoryCache(new MemoryCacheOptions());
        _exceptionsHelper = new Mock<IExceptionsHelper>();
        _config = new Mock<IConfiguration>();

        _config.Setup(s => s["secretKey"]).Returns(SecretKey);
        _microservices = InitializeMicroserviceModels.InitializeMicroservices();

        _authService = new AuthService(_logger.Object, _cache, _exceptionsHelper.Object, _config.Object);
    }

    #endregion

    #region GetTokenForMicroservice

    [TestCase(Microservice.MarvelousCrm)]
    [TestCase(Microservice.MarvelousReporting)]
    [TestCase(Microservice.MarvelousResource)]
    public void GetTokenForMicroservice_ValidRequestReceived_ShouldToken(Microservice service)
    {
        //given
        var expected = GenerateToken(service);

        //when
        var actual = _authService.GetTokenForMicroservice(service);

        //then
        Assert.AreEqual(expected.Split('.')[0], actual.Split('.')[0]);
        Assert.AreEqual(expected.Split('.')[1].Length, actual.Split('.')[1].Length);
        Assert.AreEqual(expected.Split('.')[2].Length, actual.Split('.')[2].Length);
        VerifyLogger(_logger, LogLevel.Information, 1);
    }

    #endregion

    #region GetHashPassword

    [TestCase("gaga4982")]
    [TestCase("")]
    [TestCase("     ")]
    public void GetHashPassword_ValidRequestReceived_ShouldHash(string passwordForTest)
    {
        //given
        var password = passwordForTest;
        var expected = PasswordHashTests.CalcHash(password);

        //when
        var actual = _authService.GetHashPassword(password);

        //then
        Assert.AreEqual(actual.Split(":")[0], expected.Split(":")[0]);
        Assert.AreEqual(actual.Split(":")[1].Length, expected.Split(":")[1].Length);
        Assert.AreEqual(actual.Split(":")[2].Length, expected.Split(":")[2].Length);
        Assert.AreEqual(Convert.FromBase64String(actual.Split(":")[1]).Length, PasswordHash.SaltByteSize);
        Assert.AreEqual(Convert.FromBase64String(actual.Split(":")[2]).Length, PasswordHash.HashByteSize);
    }

    #endregion

    #region GetTokenForFront

    [TestCaseSource(typeof(AuthServiceTestCaseData), nameof(AuthServiceTestCaseData.GetTestCaseDataForGetTokenForFrontTest))]
    public void GetTokenForFront_ValidRequestReceived_ShouldToken(string email, Microservice service, LeadAuthModel entity, Claim[] claims)
    {
        //given
        _cache.Set("Initialization leads", true);
        _cache.Set(email, entity);
        var expected = GenerateToken(service, claims);

        //when
        var actual = _authService.GetTokenForFront(email, "", service);

        //then
        Assert.AreEqual(expected.Split('.')[0], actual.Split('.')[0]);
        Assert.AreEqual(expected.Split('.')[1].Length, actual.Split('.')[1].Length);
        Assert.AreEqual(expected.Split('.')[2].Length, actual.Split('.')[2].Length);
        VerifyLogger(_logger, LogLevel.Information, 1);
    }

    [Test]
    public void GetTokenForFront_WhenLeadInitializationNotCompleted_ShouldThrowServiceUnavailableException()
    {
        //given
        var expected = "Microservice initialize leads was not completed";

        //when
        var actual = Assert.Throws<ServiceUnavailableException>(() => _authService.GetTokenForFront("", "", 0))!.Message;

        //then
        Assert.AreEqual(expected, actual);
        VerifyLogger(_logger, LogLevel.Error, 1);
    }

    [Test]
    public void GetTokenForFront_WhenLeadNotFoundInCache_ShouldThrowNotFoundException()
    {
        //given
        var expected = "Test exception";
        _cache.Set("Initialization leads", true);
        _exceptionsHelper.Setup(s => s.ThrowIfEmailNotFound(IsAny<string>(), IsAny<LeadAuthModel>())).Throws(new NotFoundException(expected));

        //when
        var actual = Assert.Throws<NotFoundException>(() => _authService.GetTokenForFront("test@example.com", "", 0))!.Message;

        //then
        Assert.AreEqual(expected, actual);
        VerifyLogger(_logger, IsAny<LogLevel>(), 0);
    }

    [Test]
    public void GetTokenForFront_WhenWrongPassword_ShouldThrowIncorrectPasswordException()
    {
        //given
        var expected = "Test exception";
        _cache.Set("Initialization leads", true);
        _exceptionsHelper.Setup(s => s.ThrowIfPasswordIsIncorrected(IsAny<string>(), IsAny<string>())).Throws(new IncorrectPasswordException(expected));

        //when
        var actual = Assert.Throws<IncorrectPasswordException>(() => _authService.GetTokenForFront("test@example.com", "", 0))!.Message;

        //then
        Assert.AreEqual(expected, actual);
        VerifyLogger(_logger, IsAny<LogLevel>(), 0);
    }

    #endregion

    #region CheckValidTokenAmongMicroservices

    [TestCase(Microservice.MarvelousCrm, Microservice.MarvelousAuth)]
    [TestCase(Microservice.MarvelousResource, Microservice.MarvelousCrm)]
    [TestCase(Microservice.MarvelousCrm, Microservice.MarvelousTransactionStore)]
    public void CheckValidTokenAmongMicroservices_ValidRequestReceived_ShouldNothing(Microservice issuerToken, Microservice service)
    {
        //when
        var actual = _authService.CheckValidTokenAmongMicroservices(issuerToken.ToString(), _microservices[issuerToken].ServicesThatHaveAccess, service);

        //then
        Assert.IsTrue(actual);
        VerifyLogger(_logger, LogLevel.Information, 2);
    }

    [Test]
    public void CheckValidTokenAmongMicroservices_WhenTokenContainsInvalidData_ShouldThrowAuthenticationException()
    {
        //given
        var expected = "Broken token";

        //when
        var actual = Assert.Throws<AuthenticationException>(() =>
            _authService.CheckValidTokenAmongMicroservices(Microservice.MarvelousCrm.ToString(), "", Microservice.MarvelousTransactionStore))!.Message;

        //then
        Assert.AreEqual(expected, actual);
        VerifyLogger(_logger, LogLevel.Information, 1);
        VerifyLogger(_logger, LogLevel.Error, 1);
    }

    [TestCase(Microservice.MarvelousCrm, Microservice.MarvelousReporting)]
    [TestCase(Microservice.MarvelousTransactionStore, Microservice.MarvelousReporting)]
    [TestCase(Microservice.MarvelousCrm, Microservice.MarvelousReporting)]
    public void CheckValidTokenAmongMicroservices_WhenNoAccessRights_ShouldThrowForbiddenException(Microservice issuerToken, Microservice service)
    {
        //given
        var expected = $"You don't have access to {service}";

        //when
        var actual = Assert.Throws<ForbiddenException>(() =>
            _authService.CheckValidTokenAmongMicroservices(issuerToken.ToString(), _microservices[issuerToken].ServicesThatHaveAccess, service))!.Message;

        //then
        Assert.AreEqual(expected, actual);
        VerifyLogger(_logger, LogLevel.Information, 1);
        VerifyLogger(_logger, LogLevel.Error, 1);
    }

    #endregion

    #region CheckValidTokenFrontend

    [TestCase(Microservice.MarvelousCrm)]
    [TestCase(Microservice.MarvelousResource)]
    [TestCase(Microservice.MarvelousReporting)]
    public void CheckValidTokenFrontend_ValidRequestReceived_ShouldNothing(Microservice service)
    {
        //when
        var actual = _authService.CheckValidTokenFrontend(service.ToString(), _microservices[service].ServicesThatHaveAccess, service);

        //then
        Assert.IsTrue(actual);
        VerifyLogger(_logger, LogLevel.Information, 2);
    }

    [Test]
    public void CheckValidTokenFrontend_WhenChecksNotTheIssuerOfTheToken_ShouldThrowAuthenticationException()
    {
        //given
        var expected = "Broken token";

        //when
        var actual = Assert.Throws<AuthenticationException>(() =>
            _authService.CheckValidTokenFrontend(Microservice.MarvelousCrm.ToString(), "", Microservice.MarvelousTransactionStore))!.Message;

        //then
        Assert.AreEqual(expected, actual);
        VerifyLogger(_logger, LogLevel.Information, 1);
        VerifyLogger(_logger, LogLevel.Error, 1);
    }

    [Test]
    public void CheckValidTokenFrontend_WhenNoAccessRights_ShouldThrowForbiddenException()
    {
        //given
        var expected = $"{Microservice.MarvelousFrontendCrm} does not have access";

        //when
        var actual = Assert.Throws<ForbiddenException>(() => _authService.CheckValidTokenFrontend(Microservice.MarvelousCrm.ToString(),
            _microservices[Microservice.MarvelousReporting].ServicesThatHaveAccess,
            Microservice.MarvelousCrm))!.Message;

        //then
        Assert.AreEqual(expected, actual);
        VerifyLogger(_logger, LogLevel.Information, 1);
        VerifyLogger(_logger, LogLevel.Error, 1);
    }

    #endregion

    #region CheckDoubleValidToken

    [TestCase(Microservice.MarvelousCrm)]
    [TestCase(Microservice.MarvelousResource)]
    [TestCase(Microservice.MarvelousReporting)]
    public void CheckDoubleValidToken_Frontend_ValidRequestReceived_ShouldNothing(Microservice service)
    {
        //when
        var actual = _authService.CheckDoubleValidToken(service.ToString(), _microservices[service].ServicesThatHaveAccess, service);

        //then
        Assert.IsTrue(actual);
        VerifyLogger(_logger, LogLevel.Information, 3);
    }

    [Test]
    public void CheckDoubleValidToken_Frontend_WhenNoAccessRights_ShouldThrowForbiddenException()
    {
        //given
        var expected = $"{Microservice.MarvelousFrontendCrm} does not have access";

        //when
        var actual = Assert.Throws<ForbiddenException>(() => _authService.CheckDoubleValidToken(Microservice.MarvelousCrm.ToString(),
            _microservices[Microservice.MarvelousReporting].ServicesThatHaveAccess,
            Microservice.MarvelousCrm))!.Message;

        //then
        Assert.AreEqual(expected, actual);
        VerifyLogger(_logger, LogLevel.Information, 2);
        VerifyLogger(_logger, LogLevel.Error, 1);
    }

    [TestCase(Microservice.MarvelousCrm, Microservice.MarvelousAuth)]
    [TestCase(Microservice.MarvelousResource, Microservice.MarvelousCrm)]
    [TestCase(Microservice.MarvelousCrm, Microservice.MarvelousTransactionStore)]
    public void CheckDoubleValidToken_Microservice_ValidRequestReceived_ShouldNothing(Microservice issuerToken, Microservice service)
    {
        //when
        var actual = _authService.CheckDoubleValidToken(issuerToken.ToString(), _microservices[issuerToken].ServicesThatHaveAccess, service);

        //then
        Assert.IsTrue(actual);
        VerifyLogger(_logger, LogLevel.Information, 3);
    }

    [Test]
    public void CheckDoubleValidToken_Microservice_WhenTokenContainsInvalidData_ShouldThrowAuthenticationException()
    {
        //given
        var expected = "Broken token";

        //when
        var actual = Assert.Throws<AuthenticationException>(() =>
            _authService.CheckDoubleValidToken(Microservice.MarvelousCrm.ToString(), "", Microservice.MarvelousTransactionStore))!.Message;

        //then
        Assert.AreEqual(expected, actual);
        VerifyLogger(_logger, LogLevel.Information, 2);
        VerifyLogger(_logger, LogLevel.Error, 1);
    }

    [TestCase(Microservice.MarvelousCrm, Microservice.MarvelousReporting)]
    [TestCase(Microservice.MarvelousTransactionStore, Microservice.MarvelousReporting)]
    [TestCase(Microservice.MarvelousCrm, Microservice.MarvelousReporting)]
    public void CheckDoubleValidToken_Microservice_WhenNoAccessRights_ShouldThrowForbiddenException(Microservice issuerToken, Microservice service)
    {
        //given
        var expected = $"You don't have access to {service}";

        //when
        var actual = Assert.Throws<ForbiddenException>(() =>
            _authService.CheckDoubleValidToken(issuerToken.ToString(), _microservices[issuerToken].ServicesThatHaveAccess, service))!.Message;

        //then
        Assert.AreEqual(expected, actual);
        VerifyLogger(_logger, LogLevel.Information, 2);
        VerifyLogger(_logger, LogLevel.Error, 1);
    }

    #endregion

    private string GenerateToken(Microservice issuerService, IEnumerable<Claim>? claims = null)
    {
        var jwt = new JwtSecurityToken(
            issuerService.ToString(),
            _microservices[issuerService].ServicesThatHaveAccess,
            claims,
            expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(30)),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey)),
                SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}