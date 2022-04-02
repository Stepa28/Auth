using Auth.BusinessLayer.Models;
using Auth.BusinessLayer.Security;
using Auth.BusinessLayer.Services;
using Marvelous.Contracts.Enums;
using Marvelous.Contracts.RequestModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Swashbuckle.AspNetCore.Annotations;

namespace Auth.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthorizationsController : Controller
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthorizationsController> _logger;
    private readonly IMemoryCache _cache;

    public AuthorizationsController(IAuthService authService, ILogger<AuthorizationsController> logger, IMemoryCache cache)
    {
        _authService = authService;
        _logger = logger;
        _cache = cache;
    }

    //api/auth/login
    [HttpPost("login")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [SwaggerOperation("Get token")]
    public async Task<ActionResult> Login([FromBody] AuthRequestModel auth)
    {
        //var gg = (HttpContext.User.Identity as ClaimsIdentity)!.FindFirst("aud")!.Value; //вытаскивание Audience из токена
        //var gg = (HttpContext.User.Identity as ClaimsIdentity)!.FindFirst("aud")!.Issuer; //вытаскивание Issuer из токена 
        var service = _cache.Get<Dictionary<Microservice, MicroserviceModel>>(nameof(Microservice))
                            .Values
                            .Single(t => t.Ip == HttpContext.Connection.RemoteIpAddress!.ToString())
                            .Microservice;
        
        _logger.LogInformation($"Poluchen zapros na authentikaciu po email = {auth.Email.Encryptor()}.");
        var token = await _authService.GetToken(auth.Email, auth.Password, service);
        _logger.LogInformation($"Authentikacia po email = {auth.Email.Encryptor()} proshhla uspeshno.");
        
        return new JsonResult(token);
    }
}