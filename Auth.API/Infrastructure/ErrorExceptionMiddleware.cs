using System.Net;
using System.Security.Authentication;
using System.Text.Json;
using Auth.BusinessLayer.Exceptions;
using Marvelous.Contracts.ResponseModels;
using NLog;

namespace Auth.API.Infrastructure;

public class ErrorExceptionMiddleware
{
    private readonly Logger _logger;
    private readonly RequestDelegate _next;

    public ErrorExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
        _logger = LogManager.GetCurrentClassLogger();
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ForbiddenException ex)
        {
            await ConstructResponse(context, HttpStatusCode.Forbidden, ex.Message);
        }
        catch (ServiceUnavailableException ex)
        {
            await ConstructResponse(context, HttpStatusCode.ServiceUnavailable, ex.Message);
        }
        catch (AuthenticationException ex)
        {
            await ConstructResponse(context, HttpStatusCode.Unauthorized, ex.Message);
        }
        catch (NotFoundException ex)
        {
            await ConstructResponse(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (BadRequestException ex)
        {
            await ConstructResponse(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.Error(ex);
            await ConstructResponse(context, HttpStatusCode.BadRequest, ex.Message);
        }
    }

    private async Task ConstructResponse(HttpContext context, HttpStatusCode code, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        var updateModel = new ExceptionResponseModel { Code = (int)code, Message = message };

        var result = JsonSerializer.Serialize(updateModel);
        await context.Response.WriteAsync(result);
    }
}