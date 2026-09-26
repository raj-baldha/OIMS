using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;
using OIMS.API.Helpers;
using OIMS.Application.Interfaces.Repositories;
using OIMS.Domain.Entities;

namespace OIMS.API.Filters;

public class ExecutionTimeFilter : IAsyncActionFilter
{
    private readonly IApiExecutionLogRepository _repository;
    private readonly ILogger<ExecutionTimeFilter> _logger;

    public ExecutionTimeFilter(
        IApiExecutionLogRepository repository,
        ILogger<ExecutionTimeFilter> logger
    )
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next
    )
    {
        var stopwatch = Stopwatch.StartNew();

        var executedContext = await next();

        stopwatch.Stop();
        long executionTime = stopwatch.ElapsedMilliseconds;

        string requestPath = context.HttpContext.Request.Path;
        string httpMethod = context.HttpContext.Request.Method;
        string actionName = context.ActionDescriptor.DisplayName ?? "Unknown";
        int statusCode = context.HttpContext.Response.StatusCode;

        int? userId = null;
        if (context.HttpContext.User.Identity?.IsAuthenticated == true)
        {
            userId = ClaimHelper.GetUserId(context.HttpContext.User);
        }

        _logger.LogInformation(
            "API {HttpMethod} {RequestPath} executed in {ExecutionTime} ms with status code {StatusCode}",
            httpMethod,
            requestPath,
            executionTime,
            statusCode
        );

        var log = new ApiExecutionLog
        {
            UserId = userId,
            HttpMethod = httpMethod,
            RequestPath = requestPath,
            ActionName = actionName,
            StatusCode = statusCode,
            ExecutionTimeMs = executionTime,
            CreatedAt = DateTime.UtcNow,
        };

        await _repository.AddAsync(log);
        await _repository.SaveChangesAsync();
    }
}
