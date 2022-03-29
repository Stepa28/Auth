using System.Net;
using System.Text.Json;
using Auth.BusinessLayer.Exceptions;
using Marvelous.Contracts.ResponseModels;
using NLog;

namespace Auth.API.Infrastructure;

public class ErrorExceptionMiddleware
{
    private readonly ILogger<ErrorExceptionMiddleware> _logger;
    private readonly RequestDelegate _next;

    public ErrorExceptionMiddleware(RequestDelegate next, ILogger<ErrorExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ForbiddenException error)
        {
            await ConstructResponse(context, HttpStatusCode.Forbidden, error.Message);
        }
        catch (NotFoundException error)
        {
            await ConstructResponse(context, HttpStatusCode.NotFound, error.Message);
        }
        catch (BadRequestException error)
        {
            await ConstructResponse(context, HttpStatusCode.BadRequest, error.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
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