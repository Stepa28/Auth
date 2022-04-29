using Auth.API.Extensions;
using Auth.BusinessLayer.Exceptions;
using Auth.BusinessLayer.Security;
using Auth.BusinessLayer.Services;
using FluentValidation;
using Marvelous.Contracts.Endpoints;
using Marvelous.Contracts.Enums;
using Marvelous.Contracts.RequestModels;
using Marvelous.Contracts.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Annotations;

namespace Auth.API.Controllers;

[ApiController]
[Route(AuthEndpoints.ApiAuth)]
public class AuthorizationsController : Controller
{
    private readonly IAdvancedController _advancedController;
    private readonly IAuthService _authService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AuthorizationsController> _logger;
    private readonly IValidator<AuthRequestModel> _validator;

    public AuthorizationsController(IAuthService authService, ILogger<AuthorizationsController> logger, IMemoryCache cache,
        IValidator<AuthRequestModel> validator, IAdvancedController advancedController)
    {
        _authService = authService;
        _logger = logger;
        _cache = cache;
        _validator = validator;
        _advancedController = advancedController;
    }

    //api/auth/login
    [HttpPost(AuthEndpoints.Login)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status503ServiceUnavailable)]
    [SwaggerOperation("Get a token for front microservices (Only marvelous microservices)")]
    public ActionResult<string> Login([FromBody] AuthRequestModel? auth)
    {
        if (auth == null)
        {
            var ex = new BadRequestException("You must specify the table details in the request body");
            _logger.LogError(ex, ex.Message);
            throw ex;
        }
        var validationResult = _validator.Validate(auth);
        if (!validationResult.IsValid)
        {
            var ex = new ValidationException(validationResult.Errors);
            _logger.LogError(ex, ex.Message);
            throw ex;
        }

        _cache.Get<Task>("Initialization task lead").Wait();
        _advancedController.Controller = this;
        _logger.LogInformation($"Received a request to receive a token by email = {auth.Email.Encryptor()}");
        var token = _authService.GetTokenForFront(auth.Email, auth.Password, _advancedController.Service);
        _logger.LogInformation("Token sent");

        return Ok(token);
    }

    //api/auth/token-microservice
    [HttpGet(AuthEndpoints.TokenForMicroservice)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status403Forbidden)]
    [SwaggerOperation("Get a token for microservice (Only marvelous microservices)")]
    public ActionResult<string> GetTokenForMicroservice()
    {
        _advancedController.Controller = this;
        var token = _authService.GetTokenForMicroservice(_advancedController.Service);
        _logger.LogInformation("Token sent");

        return Ok(token);
    }

    //api/auth/check-validate-token-microservices
    [HttpGet(AuthEndpoints.ValidationMicroservice)]
    [ProducesResponseType(typeof(IdentityResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status403Forbidden)]
    [SwaggerOperation("Check validate token among microservices (Only marvelous microservices)")]
    public ActionResult<IdentityResponseModel> CheckTokenAmongMicroservices()
    {
        _advancedController.Controller = this;
        if (_advancedController.Issuer.Equals(Microservice.MarvelousAuth.ToString()))
        {
            _logger.LogInformation("Request received to verify token issued by MarvelousAuth");
            return Ok(_advancedController.Identity);
        }

        _authService.CheckValidTokenAmongMicroservices(_advancedController.Issuer, _advancedController.Audience, _advancedController.Service);
        return Ok(_advancedController.Identity);
    }

    //api/auth/check-validate-token-front
    [HttpGet(AuthEndpoints.ValidationFront)]
    [ProducesResponseType(typeof(IdentityResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status403Forbidden)]
    [SwaggerOperation("Check validate frontend token (Only marvelous microservices)")]
    public ActionResult<IdentityResponseModel> CheckTokenFrontend()
    {
        _advancedController.Controller = this;
        _authService.CheckValidTokenFrontend(_advancedController.Issuer, _advancedController.Audience, _advancedController.Service);
        var identity = _advancedController.Identity;

        if (identity.Id != null)
            return Ok(identity);

        var ex = new ForbiddenException($"Failed to get lead data from token ({_advancedController.Service})");
        _logger.LogWarning(ex, ex.Message);
        throw ex;
    }

    //api/auth/check-double-validate-token/
    [HttpGet(AuthEndpoints.DoubleValidation)]
    [ProducesResponseType(typeof(IdentityResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status403Forbidden)]
    [SwaggerOperation("Check double validate token (Only marvelous microservices)")]
    public ActionResult<IdentityResponseModel> DoubleCheckToken()
    {
        _advancedController.Controller = this;
        _authService.CheckDoubleValidToken(_advancedController.Issuer, _advancedController.Audience, _advancedController.Service);
        return Ok(_advancedController.Identity);
    }

    //api/auth/hashing/
    [HttpPost(AuthEndpoints.Hash)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ExceptionResponseModel), StatusCodes.Status403Forbidden)]
    [SwaggerOperation("Get the string hash of a string (Only marvelous microservices)")]
    public ActionResult<string> GetHashingString([FromBody] string? password)
    {
        _advancedController.Controller = this;
        _logger.LogInformation($"{_advancedController.Service} asked to hashing password");
        if (password.IsNullOrEmpty())
        {
            var ex = new BadRequestException("You must specify the table details in the request body");
            _logger.LogError(ex, ex.Message);
            throw ex;
        }

        var hash = _authService.GetHashPassword(password!);
        _logger.LogInformation("Hash password send");

        return Ok(hash);
    }
}