using Auth.API.Extensions;
using Auth.BusinessLayer.Security;
using Auth.BusinessLayer.Services;
using Marvelous.Contracts.Enums;
using Marvelous.Contracts.RequestModels;
using Marvelous.Contracts.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Swashbuckle.AspNetCore.Annotations;

namespace Auth.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthorizationsController : AdvancedController
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthorizationsController> _logger;

    public AuthorizationsController(IAuthService authService, ILogger<AuthorizationsController> logger,
        IMemoryCache cache, IInitializeMicroserviceModels model) : base(logger, cache, model)
    {
        _authService = authService;
        _logger = logger;
    }

    //api/auth/login
    [HttpPost("login")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status400BadRequest)]
    [SwaggerOperation("Get a token for font and microservice")]
    public async Task<ActionResult<string>> Login([FromBody] AuthRequestModel auth)
    {
        _logger.LogInformation($"Received a request to receive a token by email = {auth.Email.Encryptor()}");
        var token = await _authService.GetTokenForFront(auth.Email, auth.Password, Service);
        _logger.LogInformation("Token sent");

        return Ok(token);
    }
    
    //api/auth/token-microservice"
    [HttpGet("token-microservice")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status400BadRequest)]
    [SwaggerOperation("Get a token for microservice")]
    public async Task<ActionResult<string>> GetTokenForMicroservice()
    {
        var token = await _authService.GetTokenForMicroservice(Service);
        _logger.LogInformation("Token sent");

        return Ok(token);
    }

    //api/auth/check-validate-token-microservices
    [HttpGet("check-validate-token-microservices")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status400BadRequest)]
    [SwaggerOperation("Check validate token among microservices")]
    public async Task<ActionResult> CheckTokenAmongMicroservices()
    {
        await _authService.CheckValidTokenAmongMicroservices(Issuer, Audience, Service);
        return Ok();
    }

    //api/auth/check-validate-token-front
    [HttpGet("check-validate-token-front")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status400BadRequest)]
    [SwaggerOperation("Check validate frontend token")]
    public async Task<ActionResult> CheckTokenFrontend()
    {
        await _authService.CheckValidTokenFrontend(Issuer, Audience, Service);
        return Ok();
    }

    //api/auth/hash-password
    [HttpPost("hash-password")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status400BadRequest)]
    [SwaggerOperation("Hashing password")]
    public async Task<ActionResult<string>> GetHashPassword([FromBody] string password)
    {
        _logger.LogInformation($"{Service} asked to hashing password");
        await _authService.CheckValidTokenAmongMicroservices(Issuer, Audience, Microservice.MarvelousAuth);
        var hash = await _authService.GetHashPassword(password);
        _logger.LogInformation("Hash password send");
        
        return Ok(hash);
    }
}