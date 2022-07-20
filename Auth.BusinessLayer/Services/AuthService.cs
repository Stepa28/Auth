using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using Auth.BusinessLayer.Exceptions;
using Auth.BusinessLayer.Helpers;
using Auth.BusinessLayer.Models;
using Auth.BusinessLayer.Security;
using Auth.Resources;
using Marvelous.Contracts.Enums;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Auth.BusinessLayer.Services;

public class AuthService : IAuthService
{
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _config;
    private readonly IExceptionsHelper _exceptionsHelper;
    private readonly ILogger<AuthService> _logger;
    private readonly IStringLocalizer<ExceptionAndLogMessages> _localizer;

    public AuthService(ILogger<AuthService> logger, IMemoryCache memoryCache, IExceptionsHelper exceptionsHelper, IConfiguration config, IStringLocalizer<ExceptionAndLogMessages> localizer)
    {
        _logger = logger;
        _cache = memoryCache;
        _exceptionsHelper = exceptionsHelper;
        _config = config;
        _localizer = localizer;
    }

    private Dictionary<Microservice, MicroserviceModel> Microservices =>
        _cache.GetOrCreate(nameof(Microservice), _ => InitializeMicroserviceModels.InitializeMicroservices());

    public string GetTokenForFront(string email, string pass, Microservice service)
    {
        if (!_cache.Get<bool>("Initialization leads"))
        {
            var ex = new ServiceUnavailableException(_localizer["InitializeLeadsWasNotCompleted"]);
            _logger.LogError(ex, ex.Message);
            throw ex;
        }

        var entity = _cache.Get<LeadAuthModel>(email);
        _exceptionsHelper.ThrowIfEmailNotFound(email, entity);
        _exceptionsHelper.ThrowIfPasswordIsIncorrected(pass, entity.HashPassword);

        var claims = new Claim[]
        {
            new(ClaimTypes.UserData, entity.Id.ToString()),
            new(ClaimTypes.Role, entity.Role.ToString())
        };

        _logger.LogInformation(_localizer["ReceivedToken", email.Encryptor(), service]);
        return GenerateToken(service, claims);
    }

    public string GetTokenForMicroservice(Microservice service)
    {
        _logger.LogInformation(_localizer["ReceiveTokenForMicroservices", service]);
        return GenerateToken(service);
    }

    public bool CheckValidTokenAmongMicroservices(string issuerToken, string audienceToken, Microservice service)
    {
        _logger.LogInformation(_localizer["RequestValidateToken", service]);
        var issuerMicroserviceModel = Microservices.Values.FirstOrDefault(t => t.Microservice.ToString().Equals(issuerToken));
        if (issuerMicroserviceModel == null || !issuerMicroserviceModel.ServicesThatHaveAccess.Equals(audienceToken))
        {
            var ex = new AuthenticationException(_localizer["BrokenToken"]);
            _logger.LogError(ex, _localizer["TokenContainsInvalidData"]);
            throw ex;
        }

        var audiencesFromToken = Regex.Split(audienceToken, ",");
        if (!audiencesFromToken.Contains(service.ToString()))
        {
            var ex = new ForbiddenException(_localizer["DontHaveAccess", service]);
            _logger.LogError(ex, _localizer["NotAudiences", ex.Message]);
            throw ex;
        }

        _logger.LogInformation(_localizer["VerificationSuccessful"]);
        return true;
    }

    public bool CheckValidTokenFrontend(string issuerToken, string audienceToken, Microservice service)
    {
        _logger.LogInformation(_localizer["FrontRequestValidateToken", service]);
        if (!issuerToken.Equals(service.ToString()))
        {
            var ex = new AuthenticationException(_localizer["BrokenToken"]);
            _logger.LogError(ex, _localizer["TokenWasNotIssuedForService", service, ex.Message]);
            throw ex;
        }

        var frontendFromService = Microservices[service].Frontend.ToString();
        var audiencesFromToken = Regex.Split(audienceToken, ",");
        if (!audiencesFromToken.Contains(frontendFromService))
        {
            var ex = new ForbiddenException(_localizer["FrontendDoesntAccess", frontendFromService]);
            _logger.LogError(ex, _localizer["TokenWasNotIssuedForService", service, ex.Message]);
            throw ex;
        }

        _logger.LogInformation(_localizer["VerificationSuccessful"]);
        return true;
    }

    public bool CheckDoubleValidToken(string issuerToken, string audienceToken, Microservice service)
    {
        _logger.LogInformation(_localizer["TokenDoubleValidation", service]);

        if (issuerToken.Equals(service.ToString()))
            CheckValidTokenFrontend(issuerToken, audienceToken, service);
        else
            CheckValidTokenAmongMicroservices(issuerToken, audienceToken, service);

        return true;
    }

    public string GetHashPassword(string password)
    {
        return PasswordHash.HashPassword(password);
    }

    private string GenerateToken(Microservice issuerService, IEnumerable<Claim>? claims = null)
    {
        var jwt = new JwtSecurityToken(
            issuerService.ToString(),
            Microservices[issuerService].ServicesThatHaveAccess,
            claims,
            expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(30)),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["secretKey"])),
                SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}