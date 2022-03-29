using Auth.BusinessLayer.Security;
using Auth.BusinessLayer.Services;
using Marvelous.Contracts.RequestModels;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Auth.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthorizationsController : Controller
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthorizationsController> _logger;
    public AuthorizationsController(IAuthService authService, ILogger<AuthorizationsController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    //api/auth/login
    [HttpPost("login")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [SwaggerOperation("Get token")]
    public async Task<ActionResult> Login([FromBody] AuthRequestModel auth)
    {
        //TODO каким-то образом получать сервис который вызывает EndPoint
        _logger.LogInformation($"Poluchen zapros na authentikaciu po email = {auth.Email.Encryptor()}.");
        var token = await _authService.GetToken(auth.Email, auth.Password);
        _logger.LogInformation($"Authentikacia po email = {auth.Email.Encryptor()} proshhla uspeshno.");
        return new JsonResult(token);
    }
}