using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VovinamERP.Application.Common.Exceptions;

namespace VovinamERP.Api.ExceptionHandling;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
            _logger.LogWarning(
    "GLOBAL EXCEPTION HANDLER HIT: {ExceptionType} - {Message}",
    exception.GetType().FullName,
    exception.Message);
    
        var problemDetails = exception switch
        {
            ValidationException validationException =>
                CreateValidationProblemDetails(validationException),

            ConflictException =>
                new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Conflict",
                    Detail = exception.Message
                },

            _ =>
                new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred."
                }
        };

        if (problemDetails.Status ==
            StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "An unhandled exception occurred.");
        }
        else
        {
            _logger.LogWarning(
                exception,
                "A handled application exception occurred.");
        }

        httpContext.Response.StatusCode =
            problemDetails.Status ??
            StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);

        return true;
    }

    private static ValidationProblemDetails
        CreateValidationProblemDetails(
            ValidationException exception)
    {
        return new ValidationProblemDetails(
            exception.Errors.ToDictionary(
                x => x.Key,
                x => x.Value))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation Error",
            Detail = exception.Message
        };
    }
}