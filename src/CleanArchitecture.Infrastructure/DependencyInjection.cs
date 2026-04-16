using CleanArchitecture.Application.Abstractions.DomainEvents;
using CleanArchitecture.Application.Abstractions.IntegrationEvents;
using CleanArchitecture.Domain.Templates.Repositories;
using CleanArchitecture.Infrastructure.BackgroundServices;
using CleanArchitecture.Infrastructure.DomainEvents;
using CleanArchitecture.Infrastructure.Messaging;
using CleanArchitecture.Infrastructure.Messaging.RabbitMq;
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
        builder.AddDomainEventHandlers();
        builder.AddIntegrationEvents();

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
        builder.Services.AddSingleton<ConvertDomainEventsToOutboxMessagesInterceptor>();
        builder.Services.AddScoped<ConvertIntegrationEventsToOutboxMessagesInterceptor>();

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
            opt.AddInterceptors(sp.GetRequiredService<ConvertDomainEventsToOutboxMessagesInterceptor>());
            opt.AddInterceptors(sp.GetRequiredService<ConvertIntegrationEventsToOutboxMessagesInterceptor>());
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

    private static WebApplicationBuilder AddDomainEventHandlers(this WebApplicationBuilder builder)
    {
        builder.Services.Scan(scan => scan
            .FromAssemblies(typeof(Application.AssemblyReference).Assembly)
            .AddClasses(classes => classes.AssignableTo(typeof(IDomainEventHandler<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        builder.Services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();

        builder.Services.AddHostedService<ProcessDomainEventsJob>();

        return builder;
    }

    private static WebApplicationBuilder AddIntegrationEvents(this WebApplicationBuilder builder)
    {
        // 1. Settings & RabbitMQ Connection (Singletons)
        var rabbitSettings = new RabbitMqSettings();
        builder.Configuration.GetSection("RabbitMq").Bind(rabbitSettings);
        builder.Services.AddSingleton(rabbitSettings);

        builder.Services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();
        builder.Services.AddSingleton<IIntegrationEventBus, RabbitMqBus>();

        builder.Services.AddScoped<IntegrationEventBuffer>();
        builder.Services.AddScoped<IIntegrationEventPublisher, IntegrationEventStagingService>();

        builder.Services.AddHostedService<ProcessIntegrationEventsJob>();
        builder.Services.AddHostedService<IntegrationEventConsumerWorker>();


        return builder;
    }
}
