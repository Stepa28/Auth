using Marvelous.Contracts.Enums;
using Marvelous.Contracts.ResponseModels;
using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Extensions;

public interface IAdvancedController
{
    Microservice Service { get; }
    string Audience { get; }
    string Issuer { get; }
    IdentityResponseModel Identity { get; }
    Controller Controller { get; set; }
}