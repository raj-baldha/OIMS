using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OIMS.API.Helpers;
using OIMS.Application.Interfaces.Repositories;
using OIMS.Domain.Entities;
using OIMS.Domain.Exceptions;

namespace OIMS.API.Middlewares;

public class ExceptionMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IApiExceptionLogRepository repository)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex, repository);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception ex,
        IApiExceptionLogRepository repository
    )
    {
        if (context.Response.HasStarted)
        {
            _logger.LogWarning("Response has already started, skipping custom exception response.");
            return;
        }

        context.Response.ContentType = "application/json";

        int statusCode = (int)HttpStatusCode.InternalServerError;
        string errorCode = "INTERNAL_SERVER_ERROR";
        string message = "Something went wrong. Please try again later.";
        List<string> errors = [];

        if (ex is BaseException baseEx)
        {
            statusCode = baseEx.StatusCode;
            errorCode = baseEx.ErrorCode;
            message = baseEx.Message;
            errors = baseEx.Errors ?? [];
        }
        else if (ex is DbUpdateException)
        {
            statusCode = (int)HttpStatusCode.BadRequest;
            errorCode = "DATABASE_ERROR";
            message = "A database constraint was violated.";
            errors = ["Invalid reference or duplicate entry."];
        }
        else if (
            ex.Message.Contains(
                "The AuthorizationPolicy named:",
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            statusCode = (int)HttpStatusCode.Forbidden;
            errorCode = "FORBIDDEN";
            message = "You are not authorized to access this resource.";
            errors = ["Authorization failed."];
        }

        await LogExceptionAsync(context, ex, statusCode, errorCode, repository);

        context.Response.StatusCode = statusCode;

        var response = new
        {
            success = false,
            statusCode,
            errorCode,
            message,
            errors,
            timestamp = DateTime.UtcNow,
        };

        var json = JsonSerializer.Serialize(response, JsonOptions);
        await context.Response.WriteAsync(json);
    }

    private static async Task LogExceptionAsync(
        HttpContext context,
        Exception ex,
        int statusCode,
        string errorCode,
        IApiExceptionLogRepository repository
    )
    {
        int? userId = null;
        if (context.User.Identity?.IsAuthenticated == true)
        {
            userId = ClaimHelper.GetUserId(context.User);
        }

        var exceptionLog = new ApiExceptionLog
        {
            UserId = userId,
            HttpMethod = context.Request.Method,
            RequestPath = context.Request.Path,
            ActionName = context.GetEndpoint()?.DisplayName,
            ExceptionType = ex.GetType().Name,
            Message = ex.Message,
            StackTrace = ex.StackTrace,
            StatusCode = statusCode,
            ErrorCode = errorCode,
            CreatedAt = DateTime.UtcNow,
        };

        await repository.AddAsync(exceptionLog);
        await repository.SaveChangesAsync();
    }
}
