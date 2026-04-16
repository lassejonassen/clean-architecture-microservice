using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CleanArchitecture.SharedKernel.Messaging;

internal sealed class RequestLoggingPipelineBehavior<TRequest, TResponse>(
    ILogger<RequestLoggingPipelineBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;

        Activity.Current?.SetTag("request.name", requestName);

        logger.LogInformation("Processing request {RequestName}", requestName);

        TResponse result = await next();

        if (result.IsSuccess)
        {
            logger.LogInformation("Completed request {RequestName}", requestName);
        }
        else
        {
            Activity.Current?.SetTag("error", true);
            logger.LogError("Completed request {RequestName} with error {@Error}", requestName, result.Error);
        }

        return result;

    }
}