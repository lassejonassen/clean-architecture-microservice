using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace CleanArchitecture.Infrastructure.Logging;

public class UserEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _contextAccessor;

    public UserEnricher(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var userId = _contextAccessor.HttpContext?.User?.Identity?.Name ?? "Anonymous";
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserId", userId));
    }
}
