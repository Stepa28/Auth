using Auth.API.Extensions;
using Auth.BusinessLayer.Security;
using Auth.BusinessLayer.Services;
using Marvelous.Contracts.RequestModels;
using Marvelous.Contracts.ResponseModels;
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
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status400BadRequest)]
    [SwaggerOperation("Get token")]
    public async Task<ActionResult<string>> Login([FromBody] AuthRequestModel auth)
    {
        _logger.LogInformation($"Received a request to receive a token by email = {auth.Email.Encryptor()}");
        var service = this.GetMicroserviceWhoUseEndpointByIp(_cache, _logger);
        var token = await _authService.GetTokenForFront(auth.Email, auth.Password, service);
        _logger.LogInformation("Token sent");

        return new JsonResult(token);
    }

    //api/auth/check-validate-token-microservices
    [HttpGet("check-validate-token-microservices")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status400BadRequest)]
    [SwaggerOperation("Check validate token among microservices")]
    public async Task<ActionResult> CheckTokenAmongMicroservices()
    {
        var issuer = this.GetIssuerFromToken(_logger);
        var audience = this.GetAudienceFromToken(_logger);
        var service = this.GetMicroserviceWhoUseEndpointByIp(_cache, _logger);
        await _authService.CheckValidTokenAmongMicroservices(issuer, audience, service);
        return Ok();
    }
    
    //api/auth/check-validate-token-front
    [HttpGet("check-validate-token-front")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status400BadRequest)]
    [SwaggerOperation("Check validate token among microservices")]
    public async Task<ActionResult> CheckTokenFront()
    {
        var issuer = this.GetIssuerFromToken(_logger);
        var audience = this.GetAudienceFromToken(_logger);
        var service = this.GetMicroserviceWhoUseEndpointByIp(_cache, _logger);
        await _authService.CheckValidTokenFront(issuer, audience, service);
        return Ok();
    }
}