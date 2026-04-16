using CleanArchitecture.Infrastructure.Logging;

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

        return app;
    }
}