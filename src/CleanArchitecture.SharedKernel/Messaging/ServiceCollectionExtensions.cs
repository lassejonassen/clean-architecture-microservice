using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace CleanArchitecture.SharedKernel.Messaging;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMediator(this IServiceCollection services)
    {
        services.TryAddScoped<IMediator, Mediator>();

        // Register pipeline behavior as enumerable so multiple implementations are included.
        services.TryAddEnumerable(ServiceDescriptor.Scoped(typeof(IPipelineBehavior<,>), typeof(RequestLoggingPipelineBehavior<,>)));

        return services;
    }

    public static IServiceCollection AddMediatorHandlers(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.Scan(scan => scan
        .FromAssemblies(assemblies)
        .AddClasses(c => c.AssignableTo(typeof(IRequestHandler<,>)))
        .AsImplementedInterfaces()
        .WithScopedLifetime()
        .AddClasses(c => c.AssignableTo(typeof(IRequestHandler<>)))
        .AsImplementedInterfaces()
        .WithScopedLifetime());

        return services;
    }
}
