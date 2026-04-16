using CleanArchitecture.Application.Abstractions;
using CleanArchitecture.Infrastructure;
using CleanArchitecture.SharedKernel;
using CleanArchitecture.SharedKernel.Messaging;
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