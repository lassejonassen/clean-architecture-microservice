using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Infrastructure;

namespace CleanArchitecture.WebAPI.Middleware;

public class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string CorrelationIdHeader = "X-Correlation-ID";

    public async Task InvokeAsync(HttpContext context, ICorrelationContext correlationContext)
    {
        // Try to find the header from the caller
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var extractedId) &&
            Guid.TryParse(extractedId, out var guid))
        {
            // Cast to the setter interface (Infrastructure only)
            if (correlationContext is ICorrelationIdSetter setter)
            {
                setter.Set(guid);
            }
        }

        // Attach the ID to the response headers for the caller's benefit
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeader] = correlationContext.CorrelationId.ToString();
            return Task.CompletedTask;
        });

        await next(context);
    }
}
