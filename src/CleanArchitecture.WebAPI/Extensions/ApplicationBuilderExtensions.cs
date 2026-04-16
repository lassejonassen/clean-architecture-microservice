using CleanArchitecture.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.WebAPI.Extensions;

public static partial class ApplicationBuilderExtensions
{
    public static IApplicationBuilder MigrateDatabases(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>()
            .Database.Migrate();

        return app;
    }
}
