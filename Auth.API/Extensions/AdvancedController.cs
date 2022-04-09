using System.Security.Authentication;
using System.Security.Claims;
using System.Security.Principal;
using Auth.BusinessLayer.Exceptions;
using Auth.BusinessLayer.Helpers;
using Marvelous.Contracts.Enums;
using Marvelous.Contracts.ResponseModels;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Extensions;

public class AdvancedController : Controller
{
    protected Microservice Service => GetMicroserviceWhoUseEndpoint();
    protected string Audience => GetAudienceFromToken();
    protected string Issuer => GetIssuerFromToken();
    protected LeadIdentityResponseModel? LeadIdentity => TryGetLeadIdentityFromToken();

    private readonly ILogger _logger;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _config;

    public AdvancedController(ILogger logger, IMemoryCache cache, IConfiguration config)
    {
        _logger = logger;
        _cache = cache;
        _config = config;
    }

    private Microservice GetMicroserviceWhoUseEndpoint()
    {
        if (!_config["BaseAddress"].Equals(HttpContext.Connection.RemoteIpAddress!.ToString()))
        {
            var ex = new ForbiddenException("Your ip is not registered");
            _logger.LogError(ex, ex.Message);
            throw ex;
        }

        if (HttpContext.Request.Headers[nameof(Microservice)].Count == 0)
        {
            var ex = new ForbiddenException("Failed to identify service");
            _logger.LogError(ex, ex.Message);
            throw ex;
        }

        var tmp = _cache.GetOrCreate(nameof(Microservice),
                            (ICacheEntry _) => InitializeMicroserviceModels.InitializeMicroservices())
                        .FirstOrDefault(t => t.Key.ToString().Equals(HttpContext.Request.Headers[nameof(Microservice)][0])).Value;

        if (tmp is null)
        {
            var ex = new ForbiddenException("Failed to identify service");
            _logger.LogError(ex, ex.Message);
            throw ex;
        }

        return tmp.Microservice;
    }

    private string GetAudienceFromToken()
    {
        var claim = CheckUserIdentityContainAudience(User.Identity);
        return claim.Value;
    }

    private string GetIssuerFromToken()
    {
        var claim = CheckUserIdentityContainAudience(User.Identity);
        return claim.Issuer;
    }

    private LeadIdentityResponseModel? TryGetLeadIdentityFromToken()
    {
        if (User.Identity is not ClaimsIdentity identity)
            return null;

        return int.TryParse(identity.FindFirst(ClaimTypes.UserData)?.Value, out var userId)
            ? new LeadIdentityResponseModel { Id = userId, Role = identity.FindFirst(ClaimTypes.Role).Value }
            : null;
    }

    private Claim CheckUserIdentityContainAudience(IIdentity? userIdentity)
    {
        if (userIdentity is not ClaimsIdentity identity)
        {
            var ex = new AuthenticationException("Broken token");
            _logger.LogError(ex, "Token doesn't contain claims, possible expiration or incorrect secret key");
            throw ex;
        }

        if (identity.FindFirst("aud") is not null)
            return identity.FindFirst("aud")!;

        var e = new AuthenticationException("Broken token");
        _logger.LogError(e, "Token doesn't contain audience");
        throw e;
    }
}