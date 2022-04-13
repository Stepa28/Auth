using System.Collections.Generic;
using Marvelous.Contracts.ExchangeModels;
using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp;

namespace Auth.BusinessLayer.Test.TestCaseSources;

internal static class InitializationLeadsTestCaseData
{
    private const string AddressCrm = "https://8451689";
    private const string AddressReporting = "https://197017";

    private const string Token =
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";

    private static readonly List<LeadAuthExchangeModel> ListLeads = new()
    {
        new LeadAuthExchangeModel { Email = "test@example.com", Id = 1, Role = 2, HashPassword = "1000:465" },
        new LeadAuthExchangeModel { Email = "test2@example.com", Id = 2, Role = 1, HashPassword = "1000:78987" }
    };

    internal static IEnumerable<TestCaseData> GetTestCaseDataForInitializeLeads_WhenCrmReturnedLeadAuthExchangeModelCollectionData()
    {
        var response = new RestResponse { Content = JsonConvert.SerializeObject(ListLeads), Request = new RestRequest() };
        var responseData = new RestClient().Deserialize<IEnumerable<LeadAuthExchangeModel>>(response);

        yield return new TestCaseData(ListLeads, responseData, AddressCrm, Token);
    }

    internal static IEnumerable<TestCaseData> GetTestCaseDataForInitializeLeads_WhenCrmReturnExceptionAndReportingReturnedLeadAuthExchangeModelCollectionData()
    {
        var response = new RestResponse { Content = JsonConvert.SerializeObject(ListLeads), Request = new RestRequest() };
        var responseData = new RestClient().Deserialize<IEnumerable<LeadAuthExchangeModel>>(response);

        yield return new TestCaseData(ListLeads, responseData, AddressCrm, AddressReporting, Token);
    }

    internal static IEnumerable<TestCaseData>
        GetTestCaseDataForInitializeLeads_WhenCrmAndReportingReturnException_ShouldSendMassageAndInitializationNotCompleted()
    {
        yield return new TestCaseData(AddressCrm, AddressReporting, Token);
    }
}