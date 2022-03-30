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


    public AuthService(ILogger<AuthService> logger, IMemoryCache memoryCache)
    {
        _logger = logger;
        _cache = memoryCache;
    }

    public async Task<string> GetToken(string email, string pass)
    {
        var entity = _cache.Get<LeadAuthModel>(email);
        ExceptionsHelper.ThrowIfEmailNotFound(email, entity);
        ExceptionsHelper.ThrowIfPasswordIsIncorrected(pass, entity.HashPassword);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, email), //TODO ??? нужен ли email в токене 
            new(ClaimTypes.UserData, entity.Id.ToString()),
            new(ClaimTypes.Role, entity.Role.ToString())
        };
        _logger.LogInformation($"Polu4enie tokena pol'zovatelya c email = {email.Encryptor()}.");
        var jwt = new JwtSecurityToken(
            AuthOptions.Issuer, //TODO для каждого сервиса свой(кто издал)
            AuthOptions.Audience, //TODO для каждого сервиса свой(для кого)
            claims,
            expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(30)),
            signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
        _logger.LogInformation($"Avtorizaciya pol'zovatelya c email = {email.Encryptor()} prohla uspehno.");

        return new JwtSecurityTokenHandler().WriteToken(jwt);

    }
}