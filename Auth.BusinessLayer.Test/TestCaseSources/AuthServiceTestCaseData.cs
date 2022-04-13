using System.Collections.Generic;
using System.Security.Claims;
using Auth.BusinessLayer.Models;
using Marvelous.Contracts.Enums;
using NUnit.Framework;

namespace Auth.BusinessLayer.Test.TestCaseSources;

internal class AuthServiceTestCaseData
{
    internal static IEnumerable<TestCaseData> GetTestCaseDataForGetTokenForFrontTest()
    {
        const string email = "test@example.com";

        yield return new TestCaseData(email,
            Microservice.MarvelousCrm,
            new LeadAuthModel { Id = 2, Role = (Role)2 },
            new Claim[]
            {
                new(ClaimTypes.UserData, "2"),
                new(ClaimTypes.Role, ((Role)2).ToString())
            });
        yield return new TestCaseData(email,
            Microservice.MarvelousReporting,
            new LeadAuthModel { Id = 4, Role = (Role)1 },
            new Claim[]
            {
                new(ClaimTypes.UserData, "4"),
                new(ClaimTypes.Role, ((Role)1).ToString())
            });
        yield return new TestCaseData(email,
            Microservice.MarvelousResource,
            new LeadAuthModel { Id = 10, Role = (Role)2 },
            new Claim[]
            {
                new(ClaimTypes.UserData, "10"),
                new(ClaimTypes.Role, ((Role)2).ToString())
            });
    }
}