using Microsoft.AspNetCore.Mvc;
using Pump.Application.Common.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Pump.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            _logger.LogError(ex, "Unhandled exception");

            var isUniqueConstraintViolation = IsUniqueConstraintViolation(ex);
            var (status, type) = isUniqueConstraintViolation
                ? (StatusCodes.Status409Conflict, "https://pump.app/errors/conflict")
                : ex switch
            {
                ConflictException => (StatusCodes.Status409Conflict, "https://pump.app/errors/conflict"),
                ForbiddenException => (StatusCodes.Status403Forbidden, "https://pump.app/errors/forbidden"),
                UnauthorizedException => (StatusCodes.Status401Unauthorized, "https://pump.app/errors/unauthorized"),
                ValidationAppException => (StatusCodes.Status400BadRequest, "https://pump.app/errors/validation"),
                NotFoundException => (StatusCodes.Status404NotFound, "https://pump.app/errors/not-found"),
                _ => (StatusCodes.Status500InternalServerError, "https://pump.app/errors/server-error")
            };

            var problem = new ProblemDetails
            {
                Type = type,
                Title = status == StatusCodes.Status500InternalServerError ? "Internal Server Error"
                    : isUniqueConstraintViolation ? "Conflict" : ex.GetType().Name,
                Status = status,
                Detail = status == StatusCodes.Status500InternalServerError
                    ? "An unexpected error occurred."
                    : isUniqueConstraintViolation ? "The request conflicts with existing data."
                    : ex.Message,
                Extensions = { ["traceId"] = context.TraceIdentifier }
            };

            if (ex is ValidationAppException validationEx)
            {
                problem.Extensions["errors"] = validationEx.Errors;
            }

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(problem);
        }
    }

    private static bool IsUniqueConstraintViolation(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is SqlException sqlException && sqlException.Number is 2601 or 2627)
                return true;
        }

        return false;
    }
}
