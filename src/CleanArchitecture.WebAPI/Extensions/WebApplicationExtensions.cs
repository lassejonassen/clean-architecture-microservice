using CleanArchitecture.Infrastructure.Logging;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace CleanArchitecture.WebAPI.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseDefaults(this WebApplication app)
    {
        //app.MapDefaultEndpoints();

        app.UseInfrastructureLogging();

        app.MapOpenApi();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", "v1");
        });

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        if (app.Environment.IsDevelopment())
        {
            app.MigrateDatabases();
        }

        app.MapGet("/", () => { return "Hello World"; });

        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false // No checks, just "I am alive"
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("db") || check.Tags.Contains("broker"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        return app;
    }
}