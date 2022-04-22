using System.Collections.Generic;
using Marvelous.Contracts.ResponseModels;
using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp;

namespace Auth.BusinessLayer.Test.TestCaseSources;

internal static class InitializationConfigsTestCaseData
{
    private const string Address = "197017";

    private const string Token =
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";

    private static readonly List<ConfigResponseModel> ListConfigs = new()
    {
        new ConfigResponseModel { Key = "BaseAddress", Value = "80.78.240.4" },
        new ConfigResponseModel { Key = "Address", Value = "::1:4589" }
    };

    internal static IEnumerable<TestCaseData> GetTestCaseDataForInitializeConfigsTest()
    {
        var response = new RestResponse { Content = JsonConvert.SerializeObject(ListConfigs), Request = new RestRequest() };
        var responseData = new RestClient().Deserialize<IEnumerable<ConfigResponseModel>>(response);

        yield return new TestCaseData(ListConfigs, responseData, Address, Token);
    }

    internal static IEnumerable<TestCaseData> GetTestCaseDataForInitializeConfigs_WhenNoDataReceivedFromRequestHelper_ShouldThrowException()
    {
        yield return new TestCaseData(Address, Token);
    }
}