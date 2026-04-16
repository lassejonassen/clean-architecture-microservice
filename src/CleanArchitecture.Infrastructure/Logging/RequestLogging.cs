using Microsoft.AspNetCore.Builder;
using Serilog;

namespace CleanArchitecture.Infrastructure.Logging;

public static class RequestLogging
{
    public static void UseInfrastructureLogging(this IApplicationBuilder app)
    {
        // This captures HTTP request info (Method, Path, Status Code, Timing)
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        });
    }
}
