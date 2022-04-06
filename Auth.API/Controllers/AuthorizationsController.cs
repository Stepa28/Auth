using Auth.API.Extensions;
using Auth.BusinessLayer.Security;
using Auth.BusinessLayer.Services;
using Marvelous.Contracts.Enums;
using Marvelous.Contracts.RequestModels;
using Marvelous.Contracts.ResponseModels;
using Marvelous.Contracts.Urls;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Swashbuckle.AspNetCore.Annotations;

namespace Auth.API.Controllers;

[ApiController]
[Route(AuthUrls.ApiAuth)]
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
    [HttpPost(AuthUrls.Login)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status404NotFound)]
    [SwaggerOperation("Get a token for font and microservice")]
    public ActionResult<string> Login([FromBody] AuthRequestModel auth)
    {
        _logger.LogInformation($"Received a request to receive a token by email = {auth.Email.Encryptor()}");
        var token = _authService.GetTokenForFront(auth.Email, auth.Password, Service);
        _logger.LogInformation("Token sent");

        return Ok(token);
    }

    //api/auth/token-microservice
    [HttpGet(AuthUrls.TokenForMicroservice)]
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
    [HttpGet(AuthUrls.ValidationMicroservice)]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status403Forbidden)]
    [SwaggerOperation("Check validate token among microservices")]
    public ActionResult CheckTokenAmongMicroservices()
    {
        if(Issuer.Equals(Microservice.MarvelousAuth.ToString()))
            return Ok();
        _authService.CheckValidTokenAmongMicroservices(Issuer, Audience, Service);
        return Ok();
    }

    //api/auth/check-validate-token-front
    [HttpGet(AuthUrls.ValidationFront)]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status403Forbidden)]
    [SwaggerOperation("Check validate frontend token")]
    public ActionResult CheckTokenFrontend()
    {
        _authService.CheckValidTokenFrontend(Issuer, Audience, Service);
        return Ok();
    }

    //api/auth/hash-password
    [HttpPost(AuthUrls.HashPassword)]
    [Authorize]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status403Forbidden)]
    [SwaggerOperation("Hashing password")]
    public ActionResult<string> GetHashPassword([FromBody] string password)
    {
        _logger.LogInformation($"{Service} asked to hashing password");
        _authService.CheckValidTokenAmongMicroservices(Issuer, Audience, Microservice.MarvelousAuth);
        var hash = _authService.GetHashPassword(password);
        _logger.LogInformation("Hash password send");

        return Ok(hash);
    }
}