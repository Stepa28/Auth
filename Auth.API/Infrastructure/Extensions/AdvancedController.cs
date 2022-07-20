using System.Security.Authentication;
using System.Security.Claims;
using System.Security.Principal;
using Auth.BusinessLayer.Exceptions;
using Auth.BusinessLayer.Helpers;
using Auth.Resources;
using Marvelous.Contracts.Enums;
using Marvelous.Contracts.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;

namespace Auth.API.Extensions;

public class AdvancedController : IAdvancedController
{
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _config;
    private readonly ILogger<AdvancedController> _logger;
    private readonly IStringLocalizer<ExceptionAndLogMessages> _localizer;

#pragma warning disable CS8618
    public AdvancedController(ILogger<AdvancedController> logger, IMemoryCache cache, IConfiguration config, IStringLocalizer<ExceptionAndLogMessages> localizer)
#pragma warning restore CS8618
    {
        _logger = logger;
        _cache = cache;
        _config = config;
        _localizer = localizer;
    }

    public Microservice Service => GetMicroserviceWhoUseEndpoint();
    public string Audience => GetAudienceFromToken();
    public string Issuer => GetIssuerFromToken();
    public IdentityResponseModel Identity => IdentityFromToken();
    public Controller Controller { get; set; }

    private Microservice GetMicroserviceWhoUseEndpoint()
    {
        _cache.Get<Task>("Initialization task configs").Wait();
        if (!_config["BaseAddress"].Equals(Controller.HttpContext.Connection.RemoteIpAddress!.ToString()))
            ThrowForbiddenException("UnresolvedIp");

        if (Controller.HttpContext.Request.Headers[nameof(Microservice)].Count == 0)
            ThrowForbiddenException("FailedIdentifyNoHead");

        var tmp = _cache.GetOrCreate(nameof(Microservice), _ => InitializeMicroserviceModels.InitializeMicroservices())
                        .FirstOrDefault(t => t.Key.ToString().Equals(Controller.HttpContext.Request.Headers[nameof(Microservice)].First())).Value;

        if (tmp is null)
            ThrowForbiddenException("FailedIdentifyNoHead");

        return tmp.Microservice;
    }

    private string GetAudienceFromToken()
    {
        var claim = CheckUserIdentityContainAudience(Controller.User.Identity);
        return claim.Value;
    }

    private string GetIssuerFromToken()
    {
        var claim = CheckUserIdentityContainAudience(Controller.User.Identity);
        return claim.Issuer;
    }

    private Claim CheckUserIdentityContainAudience(IIdentity? userIdentity)
    {
        if (userIdentity is not ClaimsIdentity identity)
        {
            var ex = new AuthenticationException(_localizer["BrokenToken"]);
            _logger.LogError(ex, _localizer["BrokenTokenDecoding"]);
            throw ex;
        }

        if (identity.FindFirst("aud") is not null)
            return identity.FindFirst("aud")!;

        var e = new AuthenticationException(_localizer["BrokenToken"]);
        _logger.LogError(e, _localizer["TokenDoesntAudience"]);
        throw e;
    }

    private IdentityResponseModel IdentityFromToken()
    {
        if (Controller.User.Identity is not ClaimsIdentity identity)
        {
            var ex = new AuthenticationException(_localizer["BrokenToken"]);
            _logger.LogError(ex, _localizer["BrokenTokenDecoding"]);
            throw ex;
        }

        return int.TryParse(identity.FindFirst(ClaimTypes.UserData)?.Value, out var userId)
            ? new IdentityResponseModel { Id = userId, Role = identity.FindFirst(ClaimTypes.Role)!.Value, IssuerMicroservice = Issuer }
            : new IdentityResponseModel { IssuerMicroservice = Issuer };
    }

    private void ThrowForbiddenException(string cause)
    {
        var ex = new ForbiddenException(_localizer[cause]);
        _logger.LogError(ex, ex.Message);
        throw ex;
    }
}