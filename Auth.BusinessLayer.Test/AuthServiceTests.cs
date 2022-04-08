using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using Auth.BusinessLayer.Exceptions;
using Auth.BusinessLayer.Helpers;
using Auth.BusinessLayer.Models;
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

public class AuthServiceTests
{
    private Mock<ILogger<AuthService>> _logger;
    private IMemoryCache _cache;
    private Mock<IExceptionsHelper> _exceptionsHelper;
    private Mock<IConfiguration> _config;
    private IAuthService _authService;

    private const string _secretKey = "SuperSecretKeyForTest";
    private Dictionary<Microservice, MicroserviceModel> _microservices;

    [SetUp]
    public void SetUp()
    {
        _logger = new Mock<ILogger<AuthService>>();
        _cache = new MemoryCache(new MemoryCacheOptions());
        _exceptionsHelper = new Mock<IExceptionsHelper>();
        _config = new Mock<IConfiguration>();

        _config.Setup(s => s["secretKey"]).Returns(_secretKey);
        _microservices = InitializeMicroserviceModels.InitializeMicroservices();
        _cache.Set(nameof(Microservice), _microservices);

        _authService = new AuthService(_logger.Object, _cache, _exceptionsHelper.Object, _config.Object);
    }

    [TestCaseSource(typeof(AuthServiceTestCaseData), nameof(AuthServiceTestCaseData.GetTestCaseDataForGetTokenForFrontTest))]
    public void GetTokenForFrontTest(string email, Microservice service, LeadAuthModel entity, Claim[] claims)
    {
        //given
        _cache.Set("Initialization", true);
        _cache.Set(email, entity);
        var expected = GenerateToken(service, claims);

        //when
        var actual = _authService.GetTokenForFront(email, "", service);

        //then
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void GetTokenForFrontNegativeTest_ServiceUnavailableException()
    {
        //given
        var expected = "Microservice initialize leads was not completed";

        //when
        var actual = Assert.Throws<ServiceUnavailableException>(() => _authService.GetTokenForFront("", "", 0))!.Message;

        //then
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void GetTokenForFrontNegativeTest_NotFoundException()
    {
        //given
        var expected = "Test exception";
        _cache.Set("Initialization", true);
        _exceptionsHelper.Setup(s => s.ThrowIfEmailNotFound(IsAny<string>(), IsAny<LeadAuthModel>())).Throws(new NotFoundException(expected));

        //when
        var actual = Assert.Throws<NotFoundException>(() => _authService.GetTokenForFront("test@example.com", "", 0))!.Message;

        //then
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void GetTokenForFrontNegativeTest_IncorrectPasswordException()
    {
        //given
        var expected = "Test exception";
        _cache.Set("Initialization", true);
        _exceptionsHelper.Setup(s => s.ThrowIfPasswordIsIncorrected(IsAny<string>(), IsAny<string>())).Throws(new IncorrectPasswordException(expected));

        //when
        var actual = Assert.Throws<IncorrectPasswordException>(() => _authService.GetTokenForFront("test@example.com", "", 0))!.Message;

        //then
        Assert.AreEqual(expected, actual);
    }

    [TestCase(Microservice.MarvelousCrm)]
    [TestCase(Microservice.MarvelousReporting)]
    [TestCase(Microservice.MarvelousResource)]
    public void GetTokenForMicroserviceTest(Microservice service)
    {
        //given
        var expected = GenerateToken(service);

        //when
        var actual = _authService.GetTokenForMicroservice(service);

        //then
        Assert.AreEqual(expected, actual);
    }

    [TestCase(Microservice.MarvelousCrm, Microservice.MarvelousAuth)]
    [TestCase(Microservice.MarvelousResource, Microservice.MarvelousCrm)]
    [TestCase(Microservice.MarvelousCrm, Microservice.MarvelousTransactionStore)]
    public void CheckValidTokenAmongMicroservicesTest(Microservice issuerToken, Microservice service)
    {
        //when
        var actual = _authService.CheckValidTokenAmongMicroservices(issuerToken.ToString(), _microservices[issuerToken].ServicesThatHaveAccess, service);

        //then
        Assert.IsTrue(actual);
    }

    [Test]
    public void CheckValidTokenAmongMicroservicesNegativeTest_AuthenticationException()
    {
        //given
        var expected = "Broken token";

        //when
        var actual = Assert.Throws<AuthenticationException>(() =>
            _authService.CheckValidTokenAmongMicroservices(Microservice.MarvelousCrm.ToString(), "", Microservice.MarvelousTransactionStore))!.Message;

        //then
        Assert.AreEqual(expected, actual);
    }

    [TestCase(Microservice.MarvelousCrm, Microservice.MarvelousReporting)]
    [TestCase(Microservice.MarvelousTransactionStore, Microservice.MarvelousReporting)]
    [TestCase(Microservice.MarvelousCrm, Microservice.MarvelousReporting)]
    public void CheckValidTokenAmongMicroservicesNegativeTest_ForbiddenException(Microservice issuerToken, Microservice service)
    {
        //given
        var expected = $"You don't have access to {service}";

        //when
        var actual = Assert.Throws<ForbiddenException>(() =>
            _authService.CheckValidTokenAmongMicroservices(issuerToken.ToString(), _microservices[issuerToken].ServicesThatHaveAccess, service))!.Message;

        //then
        Assert.AreEqual(expected, actual);
    }

    [TestCase(Microservice.MarvelousCrm)]
    [TestCase(Microservice.MarvelousResource)]
    [TestCase(Microservice.MarvelousReporting)]
    public void CheckValidTokenFrontendTest(Microservice service)
    {
        //when
        var actual = _authService.CheckValidTokenFrontend(service.ToString(), _microservices[service].ServicesThatHaveAccess, service);

        //then
        Assert.IsTrue(actual);
    }

    [Test]
    public void CheckValidTokenFrontendNegativeTest_AuthenticationException()
    {
        //given
        var expected = "Broken token";

        //when
        var actual = Assert.Throws<AuthenticationException>(() =>
            _authService.CheckValidTokenFrontend(Microservice.MarvelousCrm.ToString(), "", Microservice.MarvelousTransactionStore))!.Message;

        //then
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void CheckValidTokenFrontendNegativeTest_ForbiddenException()
    {
        //given
        var expected = $"{Microservice.MarvelousFrontendCrm} does not have access";

        //when
        var actual = Assert.Throws<ForbiddenException>(() => _authService.CheckValidTokenFrontend(Microservice.MarvelousCrm.ToString(),
            _microservices[Microservice.MarvelousReporting].ServicesThatHaveAccess,
            Microservice.MarvelousCrm))!.Message;

        //then
        Assert.AreEqual(expected, actual);
    }

    private string GenerateToken(Microservice issuerService, IEnumerable<Claim>? claims = null)
    {
        var jwt = new JwtSecurityToken(
            issuerService.ToString(),
            _microservices[issuerService].ServicesThatHaveAccess,
            claims,
            expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(30)),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey)),
                SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}