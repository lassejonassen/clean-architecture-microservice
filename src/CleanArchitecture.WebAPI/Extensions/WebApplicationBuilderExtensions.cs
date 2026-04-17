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
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(cfg =>
            {
                cfg
                    .AddSource(serviceName)
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.Filter = httpContext =>
                            httpContext.Request.Path != "/" &&
                            httpContext.Request.Path != "/health" &&
                            httpContext.Request.Path != "/alive";
                    })
                    .AddHttpClientInstrumentation()
                    //.AddEntityFrameworkCoreInstrumentation(options =>
                    //{
                    //    options.SetDbStatementForText = true;
                    //    options.SetDbStatementForStoredProcedure = true;
                    //})
                    .AddOtlpExporter();
            })
            .WithMetrics(cfg =>
            {
                cfg.
                    AddMeter(serviceName)
                .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddOtlpExporter((exporterOptions, metricReaderOptions) =>
                    {
                        if (builder.Environment.IsDevelopment())
                        {
                            metricReaderOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 1000;
                        }
                    });
            })
            .WithLogging(cfg =>
            {
                cfg.AddOtlpExporter();
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