using System.Net;
using System.Security.Authentication;
using System.Text.Json;
using Auth.BusinessLayer.Exceptions;
using FluentValidation;
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
        catch (AuthenticationException ex)
        {
            await ConstructResponse(context, HttpStatusCode.Unauthorized, ex.Message);
        }
        catch (ForbiddenException ex)
        {
            await ConstructResponse(context, HttpStatusCode.Forbidden, ex.Message);
        }
        catch (NotFoundException ex)
        {
            await ConstructResponse(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (BadRequestException ex)
        {
            await ConstructResponse(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (ServiceUnavailableException ex)
        {
            await ConstructResponse(context, HttpStatusCode.ServiceUnavailable, ex.Message);
        }
        catch (ValidationException ex)
        {
            await ConstructResponse(context, HttpStatusCode.UnprocessableEntity, ex.Message);
        }
        catch (IncorrectPasswordException ex)
        {
            await ConstructResponse(context, HttpStatusCode.Conflict, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.Error(ex);
            await ConstructResponse(context, HttpStatusCode.BadRequest, ex.Message);
        }
    }

    private static async Task ConstructResponse(HttpContext context, HttpStatusCode code, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        var exceptionModel = new ExceptionResponseModel { Code = (int)code, Message = message };

        var result = JsonSerializer.Serialize(exceptionModel);
        await context.Response.WriteAsync(result);
    }
}