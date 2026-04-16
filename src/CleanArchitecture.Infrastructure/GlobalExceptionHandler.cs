using CleanArchitecture.Application.Abstractions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArchitecture.Infrastructure;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    ICorrelationContext correlationContext) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // 1. Log the error with the CorrelationId context
        logger.LogError(
            exception,
            "An unhandled exception occurred. CorrelationId: {CorrelationId}",
            correlationContext.CorrelationId);

        // 2. Prepare the standardized ProblemDetails response
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Server Error",
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
            Detail = "An unexpected error occurred on our end.",
            Extensions = new Dictionary<string, object?>
            {
                // Crucial for debugging: Return the ID so the user can report it
                ["correlationId"] = correlationContext.CorrelationId
            }
        };

        // 3. Optional: Add more detail if in Development environment
        // if (env.IsDevelopment()) { problemDetails.Detail = exception.Message; }

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response
            .WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
