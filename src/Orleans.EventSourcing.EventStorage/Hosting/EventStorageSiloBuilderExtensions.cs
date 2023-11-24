using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Orleans.EventSourcing;
using Orleans.EventSourcing.EventStorage;
using Orleans.Providers;
using Orleans.Runtime;

// ReSharper disable once CheckNamespace
namespace Orleans.Hosting;

public static class EventStorageSiloBuilderExtensions
{
    public static ISiloBuilder AddEventStorageBasedLogConsistencyProviderAsDefault(this ISiloBuilder builder)
    {
        return builder.AddEventStorageBasedLogConsistencyProvider("Default");
    }

    public static ISiloBuilder AddEventStorageBasedLogConsistencyProvider(
        this ISiloBuilder builder,
        string name = "EventStream"
    )
    {
        return builder.ConfigureServices(
            services => services.AddEventStorageBasedLogConsistencyProvider(name)
        );
    }

    internal static IServiceCollection AddEventStorageBasedLogConsistencyProvider(
        this IServiceCollection services,
        string name
    )
    {
        services.AddProtocolServices();
        services.TryAddSingleton<ILogViewAdaptorFactory>(sp =>
            sp.GetServiceByName<ILogViewAdaptorFactory>(ProviderConstants.DEFAULT_LOG_CONSISTENCY_PROVIDER_NAME)
        );
        return services.AddSingletonNamedService<ILogViewAdaptorFactory, LogConsistencyProvider>(name);
    }

    private static void AddProtocolServices(this IServiceCollection services)
    {
        services.TryAddSingleton<Factory<IGrainContext, ILogConsistencyProtocolServices>>(serviceProvider =>
        {
            var factory = ActivatorUtilities.CreateFactory(typeof(ProtocolServices), new[] { typeof(IGrainContext) });
            return grainContext =>
                (ILogConsistencyProtocolServices)factory(serviceProvider, new object[] { grainContext });
        });
    }
}