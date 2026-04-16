using CleanArchitecture.Domain.Templates.Repositories;
using CleanArchitecture.Infrastructure.Persistence.DbContexts;
using CleanArchitecture.Infrastructure.Persistence.Interceptors;
using CleanArchitecture.Infrastructure.Persistence.Repositories;
using CleanArchitecture.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CleanArchitecture.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPersistence(configuration);

        return services;
    }

    private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("Database");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString), "The connection string to the database is not set");
        }

        services.AddSingleton<SetUpdatedAtInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, opt) =>
        {
            opt.UseSqlServer(connectionString, x =>
            {
                x.EnableRetryOnFailure();
                x.MigrationsHistoryTable("__EFMigrationsHistory");
            });

            if (sp.GetRequiredService<IHostEnvironment>().IsDevelopment())
            {
                opt.EnableSensitiveDataLogging();
            }

            opt.AddInterceptors(sp.GetRequiredService<SetUpdatedAtInterceptor>());
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<ITemplateRepository, TemplateRepository>();


        return services;
    }
}
