using System;
using System.Collections.Generic;
using Auth.BusinessLayer.Helpers;
using Auth.BusinessLayer.Producers;
using Auth.BusinessLayer.Services;
using Auth.BusinessLayer.Test.TestCaseSources;
using Marvelous.Contracts.Endpoints;
using Marvelous.Contracts.Enums;
using Marvelous.Contracts.ResponseModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RestSharp;
using static Moq.It;

namespace Auth.BusinessLayer.Test;

public class InitializationConfigsTests : VerifyHelper
{

    #region SetUp

    #pragma warning disable CS8618
    private Mock<IAuthService> _authService;
    private IConfiguration _config;
    private IInitializationConfigs _initializationConfigs;
    private Mock<ILogger<InitializationConfigs>> _logger;
    private Mock<IAuthProducer> _producer;
    private Mock<IRequestHelper<ConfigResponseModel>> _requestHelper;
    #pragma warning restore CS8618

    [SetUp]
    public void SetUp()
    {
        _authService = new Mock<IAuthService>();
        _config = new ConfigurationBuilder().AddJsonFile("appsettings.Test.json").AddInMemoryCollection(new Dictionary<string, string>()).Build();
        _logger = new Mock<ILogger<InitializationConfigs>>();
        _producer = new Mock<IAuthProducer>();
        _requestHelper = new Mock<IRequestHelper<ConfigResponseModel>>();
        _initializationConfigs = new InitializationConfigs(_config, _logger.Object, _requestHelper.Object, _authService.Object, _producer.Object);
    }

    #endregion

    #region InitializeConfigs

    [TestCaseSource(typeof(InitializationConfigsTestCaseData), nameof(InitializationConfigsTestCaseData.GetTestCaseDataForInitializeConfigsTest))]
    public void InitializeConfigs_ValidRequestReceived_ShouldApplyConfigs(List<ConfigResponseModel> listConfigs,
        RestResponse<IEnumerable<ConfigResponseModel>> responseData, string address, string token)
    {
        //given
        _config[$"{Microservice.MarvelousConfigs}Url"] = address;
        _authService.Setup(s => s.GetTokenForMicroservice(IsAny<Microservice>())).Returns(token);
        _requestHelper.Setup(s => s.SendRequest(IsAny<string>(), IsAny<Microservice>(), IsAny<string>())).ReturnsAsync(responseData);

        //when
        _initializationConfigs.InitializeConfigs();

        //then
        _requestHelper.Verify(v => v.SendRequest(address + ConfigsEndpoints.Configs, Microservice.MarvelousConfigs, token), Times.Once);
        _authService.Verify(v => v.GetTokenForMicroservice(Microservice.MarvelousAuth), Times.Once);
        _producer.Verify(v => v.NotifyErrorByEmail(IsAny<string>()), Times.Never);
        Assert.AreEqual(_config["BaseAddress"], listConfigs[0].Value);
        Assert.AreEqual(_config["Address"], listConfigs[1].Value);
        Assert.AreEqual(_config[$"{Microservice.MarvelousCrm}Url"], "https://piter-education.ru:5050");
        VerifyLogger(_logger, LogLevel.Information, 2);
    }

    [TestCaseSource(typeof(InitializationConfigsTestCaseData),
        nameof(InitializationConfigsTestCaseData.GetTestCaseDataForInitializeConfigs_WhenNoDataReceivedFromRequestHelper_ShouldThrowException))]
    public void InitializeConfigs_WhenNoDataReceivedFromRequestHelper_ShouldThrowException(string address, string token)
    {
        //given
        _config[$"{Microservice.MarvelousConfigs}Url"] = address;
        _authService.Setup(s => s.GetTokenForMicroservice(IsAny<Microservice>())).Returns(token);
        _requestHelper.Setup(s => s.SendRequest(IsAny<string>(), IsAny<Microservice>(), IsAny<string>())).Throws<Exception>();

        //when
        _initializationConfigs.InitializeConfigs();

        //then
        _requestHelper.Verify(v => v.SendRequest(address + ConfigsEndpoints.Configs, Microservice.MarvelousConfigs, token), Times.Once);
        _authService.Verify(v => v.GetTokenForMicroservice(Microservice.MarvelousAuth), Times.Once);
        _producer.Verify(v => v.NotifyErrorByEmail(IsAny<string>()), Times.Once);
        Assert.AreEqual(_config["BaseAddress"], "80.78.240.16");
        Assert.IsNull(_config["Address"]);
        Assert.AreEqual(_config[$"{Microservice.MarvelousCrm}Url"], "https://piter-education.ru:5050");
        VerifyLogger(_logger, LogLevel.Information, 1);
        VerifyLogger(_logger, LogLevel.Warning, 1);
    }

    #endregion

}