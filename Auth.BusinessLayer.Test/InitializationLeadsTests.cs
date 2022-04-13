using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Auth.BusinessLayer.Configuration;
using Auth.BusinessLayer.Helpers;
using Auth.BusinessLayer.Models;
using Auth.BusinessLayer.Producers;
using Auth.BusinessLayer.Services;
using Auth.BusinessLayer.Test.TestCaseSources;
using AutoMapper;
using Marvelous.Contracts.Endpoints;
using Marvelous.Contracts.Enums;
using Marvelous.Contracts.ExchangeModels;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RestSharp;
using static Moq.It;

namespace Auth.BusinessLayer.Test;

public class InitializationLeadsTests : LoggerVerifyHelper
{

    #region SetUp

    #pragma warning disable CS8618
    private Mock<IAuthService> _authService;
    private IMemoryCache _cache;
    private IConfiguration _config;
    private IInitializationLeads _initializationLeads;
    private Mock<ILogger<InitializationLeads>> _logger;
    private IMapper _mapper;
    private Mock<IAuthProducer> _producer;
    private Mock<IRequestHelper<LeadAuthExchangeModel>> _requestHelper;
    #pragma warning restore CS8618

    [SetUp]
    public void SetUp()
    {
        _logger = new Mock<ILogger<InitializationLeads>>();
        _authService = new Mock<IAuthService>();
        _producer = new Mock<IAuthProducer>();
        _requestHelper = new Mock<IRequestHelper<LeadAuthExchangeModel>>();
        _mapper = new Mapper(new MapperConfiguration(config => config.AddProfile<AutoMapperProfile>()));
        _cache = new MemoryCache(new MemoryCacheOptions());
        _config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>()).Build();

        _initializationLeads = new InitializationLeads(_requestHelper.Object, _logger.Object, _mapper, _cache, _producer.Object, _authService.Object, _config);
    }

    #endregion

    #region InitializeLeads

    [TestCaseSource(typeof(InitializationLeadsTestCaseData),
        nameof(InitializationLeadsTestCaseData.GetTestCaseDataForInitializeLeads_WhenCrmReturnedLeadAuthExchangeModelCollectionData))]
    public async Task InitializeLeads_WhenCrmReturnedLeadAuthExchangeModelCollectionData(List<LeadAuthExchangeModel> listLeads,
        RestResponse<IEnumerable<LeadAuthExchangeModel>> response, string addressCrm, string token)
    {
        //given
        _config[$"{Microservice.MarvelousCrm}Url"] = addressCrm;
        _authService.Setup(s => s.GetTokenForMicroservice(IsAny<Microservice>())).Returns(token);
        _requestHelper.Setup(s => s.SendRequest(IsAny<string>(), IsAny<string>(), IsAny<Microservice>(), IsAny<string>())).ReturnsAsync(response);

        //when
        await _initializationLeads.InitializeLeads();

        //then
        _requestHelper.Verify(v => v.SendRequest(addressCrm, CrmEndpoints.LeadApi + CrmEndpoints.Auth, Microservice.MarvelousCrm, token), Times.Once);
        _authService.Verify(v => v.GetTokenForMicroservice(Microservice.MarvelousAuth), Times.Once);
        _producer.Verify(v => v.NotifyErrorByEmail(IsAny<string>()), Times.Never);
        Assert.AreEqual(_cache.Get<LeadAuthModel>(listLeads[0].Email), _mapper.Map<LeadAuthModel>(listLeads[0]));
        Assert.AreEqual(_cache.Get<LeadAuthModel>(listLeads[1].Email), _mapper.Map<LeadAuthModel>(listLeads[1]));
        Assert.IsTrue(_cache.Get<bool>("Initialization leads"));
        Verify(_logger, LogLevel.Information, 3);
    }

    [TestCaseSource(typeof(InitializationLeadsTestCaseData),
        nameof(InitializationLeadsTestCaseData.GetTestCaseDataForInitializeLeads_WhenCrmReturnExceptionAndReportingReturnedLeadAuthExchangeModelCollectionData))]
    public async Task InitializeLeads_WhenCrmReturnExceptionAndReportingReturnedLeadAuthExchangeModelCollectionData(List<LeadAuthExchangeModel> listLeads,
        RestResponse<IEnumerable<LeadAuthExchangeModel>> response, string addressCrm, string addressRep, string token)
    {
        //given
        _config[$"{Microservice.MarvelousCrm}Url"] = addressCrm;
        _config[$"{Microservice.MarvelousReporting}Url"] = addressRep;
        _authService.Setup(s => s.GetTokenForMicroservice(IsAny<Microservice>())).Returns(token);
        _requestHelper.Setup(s => s.SendRequest(addressCrm, IsAny<string>(), IsAny<Microservice>(), IsAny<string>())).Throws<Exception>();
        _requestHelper.Setup(s => s.SendRequest(addressRep, IsAny<string>(), IsAny<Microservice>(), IsAny<string>())).ReturnsAsync(response);

        //when
        await _initializationLeads.InitializeLeads();

        //then
        _requestHelper.Verify(v => v.SendRequest(addressCrm, CrmEndpoints.LeadApi + CrmEndpoints.Auth, Microservice.MarvelousCrm, token), Times.Once);
        _requestHelper.Verify(
            v => v.SendRequest(addressRep, ReportingEndpoints.ApiLeads + ReportingEndpoints.GetAllLeads, Microservice.MarvelousReporting, token),
            Times.Once);
        _authService.Verify(v => v.GetTokenForMicroservice(Microservice.MarvelousAuth), Times.Once);
        _producer.Verify(v => v.NotifyErrorByEmail(IsAny<string>()), Times.Never);
        Assert.AreEqual(_cache.Get<LeadAuthModel>(listLeads[0].Email), _mapper.Map<LeadAuthModel>(listLeads[0]));
        Assert.AreEqual(_cache.Get<LeadAuthModel>(listLeads[1].Email), _mapper.Map<LeadAuthModel>(listLeads[1]));
        Assert.IsTrue(_cache.Get<bool>("Initialization leads"));
        Verify(_logger, LogLevel.Information, 4);
        Verify(_logger, LogLevel.Error, 1);
    }

    [TestCaseSource(typeof(InitializationLeadsTestCaseData),
        nameof(InitializationLeadsTestCaseData
            .GetTestCaseDataForInitializeLeads_WhenCrmAndReportingReturnException_ShouldSendMassageAndInitializationNotCompleted))]
    public async Task InitializeLeads_WhenCrmAndReportingReturnException_ShouldSendMassageAndInitializationNotCompleted(string addressCrm, string addressRep,
        string token)
    {
        //given
        _config[$"{Microservice.MarvelousCrm}Url"] = addressCrm;
        _config[$"{Microservice.MarvelousReporting}Url"] = addressRep;
        _authService.Setup(s => s.GetTokenForMicroservice(IsAny<Microservice>())).Returns(token);
        _requestHelper.Setup(s => s.SendRequest(IsAny<string>(), IsAny<string>(), IsAny<Microservice>(), IsAny<string>())).Throws<Exception>();

        //when
        await _initializationLeads.InitializeLeads();

        //then
        _requestHelper.Verify(v => v.SendRequest(addressCrm, CrmEndpoints.LeadApi + CrmEndpoints.Auth, Microservice.MarvelousCrm, token), Times.Once);
        _requestHelper.Verify(
            v => v.SendRequest(addressRep, ReportingEndpoints.ApiLeads + ReportingEndpoints.GetAllLeads, Microservice.MarvelousReporting, token),
            Times.Once);
        _authService.Verify(v => v.GetTokenForMicroservice(Microservice.MarvelousAuth), Times.Once);
        _producer.Verify(v => v.NotifyErrorByEmail($"Initialization leads with {Microservice.MarvelousCrm} and {Microservice.MarvelousReporting} failed"),
            Times.Once);
        Assert.IsFalse(_cache.Get<bool>("Initialization leads"));
        Verify(_logger, LogLevel.Information, 2);
        Verify(_logger, LogLevel.Error, 2);
        Verify(_logger, LogLevel.Warning, 1);
    }

    #endregion

}