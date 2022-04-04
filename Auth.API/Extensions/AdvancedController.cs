using System.Security.Claims;
using System.Security.Principal;
using Auth.BusinessLayer.Exceptions;
using Auth.BusinessLayer.Services;
using Marvelous.Contracts.Enums;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Extensions;

public class AdvancedController : Controller
{
    protected Microservice Service => GetMicroserviceWhoUseEndpointByIp();
    protected string Audience => GetAudienceFromToken();
    protected string Issuer => GetIssuerFromToken();

    private readonly ILogger _logger;
    private readonly IMemoryCache _cache;
    private readonly IInitializeMicroserviceModels _initializeModels;

    public AdvancedController(ILogger logger, IMemoryCache cache, IInitializeMicroserviceModels initializeModels)
    {
        _logger = logger;
        _cache = cache;
        _initializeModels = initializeModels;
    }

    private Microservice GetMicroserviceWhoUseEndpointByIp()
    {
        var tmp = _cache.GetOrCreate(nameof(Microservice),
                            (ICacheEntry _) => _initializeModels.InitializeMicroservices())
                        .Values
                        .FirstOrDefault(t => t.Ip == HttpContext.Connection.RemoteIpAddress!.ToString());

        if (tmp is not null)
            return tmp.Microservice;

        var ex = new ForbiddenException("Your ip is not registered");
        _logger.LogError(ex, "");
        throw ex;
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

    private Claim CheckUserIdentityContainAudience(IIdentity? userIdentity)
    {
        if (userIdentity is not ClaimsIdentity identity)
        {
            var ex = new BadRequestException("Broken token");
            _logger.LogError(ex, "Token doesn't contain claims, possible expiration or incorrect secret key");
            throw ex;
        }

        if (identity.FindFirst("aud") is not null)
            return identity.FindFirst("aud")!;

        var e = new BadRequestException("Broken token");
        _logger.LogError(e, "Token doesn't contain audience");
        throw e;
    }
}