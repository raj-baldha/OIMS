using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OIMS.Domain.Exceptions;

namespace OIMS.API.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";

            int statusCode = (int)HttpStatusCode.InternalServerError;
            string errorCode = "INTERNAL_SERVER_ERROR";
            string message = "Something went wrong. Please try again later.";
            List<string> errors = new List<string> { ex.Message };

            if (ex is BaseException baseEx)
            {
                statusCode = baseEx.StatusCode;
                errorCode = baseEx.ErrorCode;
                message = baseEx.Message;
                errors = baseEx.Errors ?? new List<string>();
            }
            else if (ex is DbUpdateException)
            {
                statusCode = (int)HttpStatusCode.BadRequest;
                errorCode = "DATABASE_ERROR";
                message = "A database constraint was violated.";
                errors = new List<string> { "Invalid reference or duplicate entry." };
            }

            context.Response.StatusCode = statusCode;

            var response = new
            {
                success = false,
                statusCode = statusCode,
                errorCode = errorCode,
                message = message,
                errors = errors,
                timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}