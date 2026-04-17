using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Infrastructure;
using CleanArchitecture.SharedKernel;
using CleanArchitecture.SharedKernel.Messaging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Text.Json.Serialization;

namespace CleanArchitecture.WebAPI.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddDefaults(this WebApplicationBuilder builder, string serviceName)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
        {
            serviceName = builder.Environment.ApplicationName;
        }

        builder.Services.AddSingleton<CorrelationContext>();
        builder.Services.AddSingleton<ICorrelationContext>(sp => sp.GetRequiredService<CorrelationContext>());
        builder.Services.AddSingleton<ICorrelationIdSetter>(sp => sp.GetRequiredService<CorrelationContext>());

        builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        builder.Services.AddMediator();
        builder.Services.AddProblemDetails();



        builder.Services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(serviceName))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(opt =>
                {
                    opt.Endpoint = new Uri("http://aspire-dashboard:4317");
                }))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation() // <--- Needs the NuGet package above
                .AddOtlpExporter(opt =>
                {
                    opt.Endpoint = new Uri("http://aspire-dashboard:4317");
                }));

        var resourceBuilder = ResourceBuilder.CreateDefault()
                .AddService(serviceName);

        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.SetResourceBuilder(resourceBuilder);
            logging.AddOtlpExporter(opt =>
            {
                opt.Endpoint = new Uri("http://aspire-dashboard:4317");
            });
            // Optional: include attributes like "userId" in logs
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddOpenApi();

        return builder;
    }
}