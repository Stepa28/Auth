using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Auth.BusinessLayer.Configurations;
using Auth.BusinessLayer.Helpers;
using Auth.BusinessLayer.Models;
using Auth.BusinessLayer.Security;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Auth.BusinessLayer.Services;

public class AuthService : IAuthService
{
    private readonly ILogger<AuthService> _logger;
    private readonly IMemoryCache _cache;
    private readonly IExceptionsHelper _exceptionsHelper;

    public AuthService(ILogger<AuthService> logger, IMemoryCache memoryCache, IExceptionsHelper exceptionsHelper)
    {
        _logger = logger;
        _cache = memoryCache;
        _exceptionsHelper = exceptionsHelper;
    }

    public Task<string> GetToken(string email, string pass)
    {
        _logger.LogInformation($"Authorization attempt with email {email.Encryptor()}.");

        var entity = _cache.Get<LeadAuthModel>(email);
        _exceptionsHelper.ThrowIfEmailNotFound(email, entity);
        _exceptionsHelper.ThrowIfPasswordIsIncorrected(pass, entity.HashPassword);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, email), //TODO ??? нужен ли email в токене 
            new(ClaimTypes.UserData, entity.Id.ToString()),
            new(ClaimTypes.Role, entity.Role.ToString())
        };

        _logger.LogInformation($"Received a token for a lead with email {email.Encryptor()}.");

        var jwt = new JwtSecurityToken(
            AuthOptions.Issuer,   //TODO для каждого сервиса свой(кто издал)
            AuthOptions.Audience, //TODO для каждого сервиса свой(для кого)
            claims,
            expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(30)),
            signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

        _logger.LogInformation($"Authorization of lead with email {email.Encryptor()} was successful.");

        return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(jwt));
    }
}