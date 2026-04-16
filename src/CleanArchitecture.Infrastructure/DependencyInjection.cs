using CleanArchitecture.Application.Abstractions;
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
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using RabbitMQ.Client;
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
        builder.AddResilience();
        builder.AddHealthChecks();

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

    private static WebApplicationBuilder AddResilience(this WebApplicationBuilder builder)
    {
        builder.Services.AddResiliencePipeline("rabbitmq-connection", builder =>
        {
            builder.AddRetry(new RetryStrategyOptions
            {
                // Handle all exceptions or specify SocketException/BrokerUnreachableException
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),

                MaxRetryAttempts = 5,

                // Exponential backoff: 2s, 4s, 8s, 16s...
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(2),

                // Add "Jitter" to prevent all services from retrying at the exact same time
                UseJitter = true,

                OnRetry = args =>
                {
                    // Log the retry attempt so you can see it in Docker logs
                    // Accessing logger here requires a different setup, or just use Console for now
                    Console.WriteLine($"RabbitMQ Connection Attempt {args.AttemptNumber} failed. Retrying...");
                    return default;
                }
            });

            builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                FailureRatio = 0.5, // Trip if 50% of attempts fail
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 10,
                BreakDuration = TimeSpan.FromSeconds(30) // Stop trying for 30s
            });
        });


        return builder;
    }

    private static WebApplicationBuilder AddHealthChecks(this WebApplicationBuilder builder)
    {
        var settings = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqSettings>()!;

        builder.Services.AddHealthChecks()
           .AddSqlServer(
               connectionString: builder.Configuration.GetConnectionString("Database")!,
               name: "sqlserver",
               tags: ["db", "data"])
           .AddRabbitMQ(
                sp =>
                {
                    // Manually resolve settings inside the factory
                    var settings = sp.GetRequiredService<IOptions<RabbitMqSettings>>().Value;
                    var connection = new ConnectionFactory
                    {
                        HostName = settings.HostName,
                        UserName = settings.UserName,
                        Password = settings.Password
                    };

                    return connection.CreateConnectionAsync();
                },
                name: "rabbitmq",
                tags: ["broker", "ready"]);

        return builder;
    }
}
