using System.Reflection;
using CleanArchitectureShowcase.Application.Common.Behaviors;
using CleanArchitectureShowcase.Application.Common.Events;
using CleanArchitectureShowcase.Application.Common.Messaging;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitectureShowcase.Application;

/// <summary>
/// Dependency injection composition extensions for the Application layer.
/// Configures the hand-written CQRS dispatcher, domain event router, pipeline behaviors,
/// and uses zero-dependency assembly scanning to register use-case handlers.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all application use case handlers, validators, behaviors, and dispatchers.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureLoggingOptions">Optional action to configure pipeline logging behavior options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        Action<LoggingBehaviorOptions>? configureLoggingOptions = null)
    {
        Assembly assembly = typeof(DependencyInjection).Assembly;

        // Register all FluentValidation validators discovered in this assembly
        services.AddValidatorsFromAssembly(assembly);

        // Hand-written assembly scanning to register CQRS and domain event handlers without third-party libraries (e.g. Scrutor)
        RegisterHandlers(services, assembly);

        // Register the zero-dependency CQRS and Domain Event dispatchers
        services.AddScoped<IDispatcher, Dispatcher>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        // Register pipeline behaviors in strict execution order:
        // 1. LoggingBehavior (outermost, measures entire pipeline execution time)
        // 2. ValidationBehavior (short-circuits invalid requests before handlers execute)
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Options binding for logging threshold
        if (configureLoggingOptions is not null)
        {
            services.Configure(configureLoggingOptions);
        }
        else
        {
            services.AddOptions<LoggingBehaviorOptions>();
        }

        return services;
    }

    private static void RegisterHandlers(IServiceCollection services, Assembly assembly)
    {
        var targetInterfaceDefinitions = new HashSet<Type>
        {
            typeof(ICommandHandler<>),
            typeof(ICommandHandler<,>),
            typeof(IQueryHandler<,>),
            typeof(IDomainEventHandler<>)
        };

        Type[] types = assembly.GetTypes();
        foreach (Type type in types)
        {
            if (!type.IsClass || type.IsAbstract)
            {
                continue;
            }

            foreach (Type iface in type.GetInterfaces())
            {
                if (iface.IsGenericType && targetInterfaceDefinitions.Contains(iface.GetGenericTypeDefinition()))
                {
                    services.AddScoped(iface, type);
                }
            }
        }
    }
}
