using Auth.API.Extensions;
using Auth.BusinessLayer.Exceptions;
using Auth.BusinessLayer.Security;
using Auth.BusinessLayer.Services;
using Marvelous.Contracts.Endpoints;
using Marvelous.Contracts.Enums;
using Marvelous.Contracts.RequestModels;
using Marvelous.Contracts.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Swashbuckle.AspNetCore.Annotations;

namespace Auth.API.Controllers;

[ApiController]
[Route(AuthEndpoints.ApiAuth)]
public class AuthorizationsController : AdvancedController
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthorizationsController> _logger;
    private readonly IMemoryCache _cache;

    public AuthorizationsController(IAuthService authService, ILogger<AuthorizationsController> logger, IMemoryCache cache, IConfiguration config)
        : base(logger, cache, config)
    {
        _authService = authService;
        _logger = logger;
        _cache = cache;
    }

    //api/auth/login
    [HttpPost(AuthEndpoints.Login)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status404NotFound)]
    [SwaggerOperation("Get a token for font and microservice")]
    public ActionResult<string> Login([FromBody] AuthRequestModel auth)
    {
        _cache.Get<Task>("Initialization task lead").Wait();
        _logger.LogInformation($"Received a request to receive a token by email = {auth.Email.Encryptor()}");
        var token = _authService.GetTokenForFront(auth.Email, auth.Password, Service);
        _logger.LogInformation("Token sent");

        return Ok(token);
    }

    //api/auth/token-microservice
    [HttpGet(AuthEndpoints.TokenForMicroservice)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status401Unauthorized)]
    [SwaggerOperation("Get a token for microservice")]
    public ActionResult<string> GetTokenForMicroservice()
    {
        var token = _authService.GetTokenForMicroservice(Service);
        _logger.LogInformation("Token sent");

        return Ok(token);
    }

    //api/auth/check-validate-token-microservices
    [HttpGet(AuthEndpoints.ValidationMicroservice)]
    [ProducesResponseType(typeof(IdentityResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status403Forbidden)]
    [SwaggerOperation("Check validate token among microservices")]
    public ActionResult CheckTokenAmongMicroservices()
    {
        if (Issuer.Equals(Microservice.MarvelousAuth.ToString()))
        {
            _logger.LogInformation("Request received to verify token issued by MarvelousAuth");
            return Ok(Identity);
        }
        
        _authService.CheckValidTokenAmongMicroservices(Issuer, Audience, Service);
        return Ok(Identity);
    }

    //api/auth/check-validate-token-front
    [HttpGet(AuthEndpoints.ValidationFront)]
    [ProducesResponseType(typeof(IdentityResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status403Forbidden)]
    [SwaggerOperation("Check validate frontend token")]
    public ActionResult CheckTokenFrontend()
    {
        _authService.CheckValidTokenFrontend(Issuer, Audience, Service);
        var identity = Identity;

        if (identity.Id != null)
            return Ok(identity);

        var ex = new ForbiddenException($"Failed to get lead data from token ({Service})");
        _logger.LogWarning(ex, ex.Message);
        throw ex;
    }
    
    //api/auth/check-double-validate-token/
    [HttpGet(AuthEndpoints.DoubleValidation)]
    [ProducesResponseType(typeof(IdentityResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status403Forbidden)]
    [SwaggerOperation("Check double validate token")]
    public ActionResult DoubleCheckToken()
    {
        _authService.CheckDoubleValidToken(Issuer, Audience, Service);
        return Ok(Identity);
    }

    //api/auth/hashing/
    [HttpPost(AuthEndpoints.Hash)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status403Forbidden)]
    [SwaggerOperation("Get the string hash of a string")]
    public ActionResult<string> GetHashingString([FromBody] string password)
    {
        _logger.LogInformation($"{Service} asked to hashing password");
        var hash = _authService.GetHashPassword(password);
        _logger.LogInformation("Hash password send");

        return Ok(hash);
    }
}