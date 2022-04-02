using System.Security.Claims;
using System.Security.Principal;
using Auth.BusinessLayer.Exceptions;
using Auth.BusinessLayer.Models;
using Marvelous.Contracts.Enums;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Extensions;

public static class ControllerExtensions
{
    public static Microservice GetMicroserviceWhoUseEndpointByIp(this Controller controller, IMemoryCache cache, ILogger logger)
    {
        var tmp = cache.Get<Dictionary<Microservice, MicroserviceModel>>(nameof(Microservice))
                       .Values
                       .FirstOrDefault(t => t.Ip == controller.HttpContext.Connection.RemoteIpAddress!.ToString());

        if (tmp is not null)
            return tmp.Microservice;

        var ex = new ForbiddenException("Your ip is not registered");
        logger.LogError(ex, "");
        throw ex;
    }

    public static string GetAudienceFromToken(this Controller controller, ILogger logger)
    {
        var claim = CheckUserIdentityContainAudience(controller.User.Identity, logger);
        return claim.Value;
    }

    public static string GetIssuerFromToken(this Controller controller, ILogger logger)
    {
        var claim = CheckUserIdentityContainAudience(controller.User.Identity, logger);
        return claim.Issuer;
    }

    private static Claim CheckUserIdentityContainAudience(IIdentity? userIdentity, ILogger logger)
    {
        if (userIdentity is not ClaimsIdentity identity)
        {
            var ex = new BadRequestException("Broken token");
            logger.LogError(ex, "Token doesn't contain claims");
            throw ex;
        }

        if (identity.FindFirst("aud") is not null)
            return identity.FindFirst("aud")!;

        var e = new BadRequestException("Broken token");
        logger.LogError(e, "Token doesn't contain audience");
        throw e;
    }
}