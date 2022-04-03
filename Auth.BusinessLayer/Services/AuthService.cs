using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Auth.BusinessLayer.Configurations;
using Auth.BusinessLayer.Exceptions;
using Auth.BusinessLayer.Helpers;
using Auth.BusinessLayer.Models;
using Auth.BusinessLayer.Security;
using Marvelous.Contracts.Enums;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Auth.BusinessLayer.Services;

public class AuthService : IAuthService
{
    private readonly ILogger<AuthService> _logger;
    private readonly IMemoryCache _cache;
    private readonly IExceptionsHelper _exceptionsHelper;
    private const string Front = "Front"; //TODO убрать магические числа

    public AuthService(ILogger<AuthService> logger, IMemoryCache memoryCache, IExceptionsHelper exceptionsHelper)
    {
        _logger = logger;
        _cache = memoryCache;
        _exceptionsHelper = exceptionsHelper;
    }

    public Task<string> GetTokenForFront(string email, string pass, Microservice service)
    {
        _logger.LogInformation($"Authorization attempt with email = {email.Encryptor()}({service})");

        var entity = _cache.Get<LeadAuthModel>(email);
        _exceptionsHelper.ThrowIfEmailNotFound(email, entity);
        _exceptionsHelper.ThrowIfPasswordIsIncorrected(pass, entity.HashPassword);

        var claims = new Claim[]
        {
            new(ClaimTypes.UserData, entity.Id.ToString()),
            new(ClaimTypes.Role, entity.Role.ToString())
        };

        _logger.LogInformation($"Received a token for a lead with email {email.Encryptor()}({service})");
        return Task.FromResult(FormationToken(service, claims));
    }

    public Task CheckValidTokenAmongMicroservices(string issuerToken, string audienceToken, Microservice service)
    {
        _logger.LogInformation($"Received a request to validate a microservices token from {service}");
        var issuerMicroserviceModel = Microservices.FirstOrDefault(t => t.Key.ToString().Equals(issuerToken)).Value;
        if (!issuerMicroserviceModel.GetServicesThatHaveAccess().Equals(audienceToken))
        {
            var ex = new BadRequestException("Broken token");
            _logger.LogError(ex, "Token contains invalid data");
            throw ex;
        }

        var audiencesFromToken = Regex.Split(audienceToken, ",");
        if (!audiencesFromToken.Contains(service.ToString()))
        {
            var ex = new ForbiddenException($"{service} service does not have access");
            _logger.LogError(ex, "Not contain from audiences");
            throw ex;
        }

        _logger.LogInformation("Verification was successful");
        return Task.CompletedTask;
    }

    public Task CheckValidTokenFrontend(string issuerToken, string audienceToken, Microservice service)
    {
        _logger.LogInformation("Frontend token validation request received");
        if (!issuerToken.Equals(service.ToString()))
        {
            var ex = new BadRequestException("Broken token");
            _logger.LogError(ex, $"The token was not issued for {service} service");
            throw ex;
        }

        var audiencesFromToken = Regex.Split(audienceToken, ",");
        if (!audiencesFromToken.Contains(Front))
        {
            var ex = new ForbiddenException($"{Front} does not have access");
            _logger.LogError(ex, $"The token was not issued for {Front}");
            throw ex;
        }

        _logger.LogInformation("Verification was successful");
        return Task.CompletedTask;
    }

    public Task<string> GetHashPassword(string password)
    {
        return Task.FromResult(PasswordHash.HashPassword(password));
    }

    private string FormationToken(Microservice issuerService, IEnumerable<Claim>? claims = null)
    {
        var jwt = new JwtSecurityToken(
            issuerService.ToString(),
            Microservices[issuerService].GetServicesThatHaveAccess(),
            claims,
            expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(30)), //TODO магические числа
            signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(),
                SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    private Dictionary<Microservice, MicroserviceModel> Microservices =>
        _cache.GetOrCreate(nameof(Microservice), new InitializeMicroserviceModels(_logger).InitializeMicroservices);
}