using CleanArchitecture.Domain.Templates.Repositories;
using CleanArchitecture.Infrastructure.Persistence.DbContexts;
using CleanArchitecture.Infrastructure.Persistence.Interceptors;
using CleanArchitecture.Infrastructure.Persistence.Repositories;
using CleanArchitecture.SharedKernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace CleanArchitecture.Infrastructure;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
    {
        builder.AddPersistence();
        builder.AddLogging();

        return builder;
    }

    private static WebApplicationBuilder AddPersistence(this WebApplicationBuilder builder)
    {
        string? connectionString = builder.Configuration.GetConnectionString("Database");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString), "The connection string to the database is not set");
        }

        builder.Services.AddSingleton<SetUpdatedAtInterceptor>();

        builder.Services.AddDbContext<ApplicationDbContext>((sp, opt) =>
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

        builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());
        builder.Services.AddScoped<ITemplateRepository, TemplateRepository>();


        return builder;
    }

    private static WebApplicationBuilder AddLogging(this WebApplicationBuilder builder)
    {
        // 1. Setup the "Bootstrap" logger for startup failures
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        // 2. Use Serilog and read from appsettings.json
        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());

        return builder;
    }
}
