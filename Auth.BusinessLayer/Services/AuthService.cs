using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using Auth.BusinessLayer.Exceptions;
using Auth.BusinessLayer.Helpers;
using Auth.BusinessLayer.Models;
using Auth.BusinessLayer.Security;
using Marvelous.Contracts.Enums;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Auth.BusinessLayer.Services;

public class AuthService : IAuthService
{
    private readonly ILogger<AuthService> _logger;
    private readonly IMemoryCache _cache;
    private readonly IExceptionsHelper _exceptionsHelper;
    private readonly IInitializeMicroserviceModels _initializeModels;
    private readonly IConfiguration _config;

    public AuthService(ILogger<AuthService> logger, IMemoryCache memoryCache, IExceptionsHelper exceptionsHelper, IInitializeMicroserviceModels initializeModels,
        IConfiguration config)
    {
        _logger = logger;
        _cache = memoryCache;
        _exceptionsHelper = exceptionsHelper;
        _initializeModels = initializeModels;
        _config = config;
    }

    public string GetTokenForFront(string email, string pass, Microservice service)
    {
        if (!_cache.Get<bool>("Initialization"))
        {
            var ex = new ServiceUnavailableException("Microservice initialize leads was not completed");
            _logger.LogError(ex, ex.Message);
            throw ex;
        }

        _logger.LogInformation($"Authorization attempt with email = {email.Encryptor()}({service})");

        var entity = _cache.Get<LeadAuthModel>(email);
        _exceptionsHelper.ThrowIfEmailNotFound(email, entity);
        _exceptionsHelper.ThrowIfPasswordIsIncorrected(pass, entity.HashPassword);

        var claims = new Claim[]
        {
            new(ClaimTypes.UserData, entity.Id.ToString()),
            new(ClaimTypes.Role, entity.Role.ToString())
        };

        _logger.LogInformation($"Received a token for a lead with email = {email.Encryptor()}({service})");
        return GenerateToken(service, claims);
    }

    public string GetTokenForMicroservice(Microservice service)
    {
        _logger.LogInformation($"{service} service requested to receive a token for microservices");
        return GenerateToken(service);
    }

    public bool CheckValidTokenAmongMicroservices(string issuerToken, string audienceToken, Microservice service)
    {
        _logger.LogInformation($"Received a request to validate a microservices token from {service}");
        var issuerMicroserviceModel = Microservices.FirstOrDefault(t => t.Key.ToString().Equals(issuerToken)).Value;
        if (!issuerMicroserviceModel.ServicesThatHaveAccess.Equals(audienceToken))
        {
            var ex = new AuthenticationException("Broken token");
            _logger.LogError(ex, "Token contains invalid data");
            throw ex;
        }

        var audiencesFromToken = Regex.Split(audienceToken, ",");
        if (!audiencesFromToken.Contains(service.ToString()))
        {
            var ex = new ForbiddenException($"You don't have access to {service}");
            _logger.LogError(ex, $"Not contain from audiences ({ex.Message})");
            throw ex;
        }

        _logger.LogInformation("Verification token was successful");
        return true;
    }

    public bool CheckValidTokenFrontend(string issuerToken, string audienceToken, Microservice service)
    {
        _logger.LogInformation($"Frontend token validation request received({service})");
        if (!issuerToken.Equals(service.ToString()))
        {
            var ex = new AuthenticationException("Broken token");
            _logger.LogError(ex, $"The token was not issued for {service} service");
            throw ex;
        }

        var frontendFromService = Microservices[service].Frontend.ToString();
        var audiencesFromToken = Regex.Split(audienceToken, ",");
        if (!audiencesFromToken.Contains(frontendFromService))
        {
            var ex = new ForbiddenException($"{frontendFromService} does not have access");
            _logger.LogError(ex, $"The token was not issued for {frontendFromService} ({ex.Message})");
            throw ex;
        }

        _logger.LogInformation("Verification token was successful");
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

    private Dictionary<Microservice, MicroserviceModel> Microservices =>
        _cache.GetOrCreate(nameof(Microservice), (ICacheEntry _) => _initializeModels.InitializeMicroservices());
}