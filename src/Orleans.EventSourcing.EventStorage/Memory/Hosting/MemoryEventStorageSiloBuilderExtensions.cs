using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.EventSourcing.EventStorage;
using Orleans.EventSourcing.EventStorage.Memory;
using Orleans.Runtime;

// ReSharper disable once CheckNamespace
namespace Orleans.Hosting;

public static class MemoryEventStorageSiloBuilderExtensions
{
    /// <summary>
    /// Configure silo to use memory event storage as the default event storage.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="configureOptions">The configuration delegate.</param>
    /// <returns>The silo builder.</returns>
    public static ISiloBuilder AddMemoryEventStorageAsDefault(
        this ISiloBuilder builder,
        Action<MemoryEventStorageOptions> configureOptions
    )
    {
        return builder.AddMemoryEventStorageAsDefault(ob => ob.Configure(configureOptions));
    }

    /// <summary>
    /// Configure silo to use memory event storage as the default event storage.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="configureOptions">The configuration delegate.</param>
    /// <returns>The silo builder.</returns>
    public static ISiloBuilder AddMemoryEventStorageAsDefault(
        this ISiloBuilder builder,
        Action<OptionsBuilder<MemoryEventStorageOptions>>? configureOptions = null
    )
    {
        return builder.AddMemoryEventStorage(
            EventStorageConstants.DEFAULT_EVENT_STORAGE_PROVIDER_NAME,
            configureOptions
        );
    }

    /// <summary>
    /// Configure silo to use memory event storage as the default event storage.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="name">The name of the event storage provider. This must match the &lt;c&gt;ProviderName&lt;/c&gt; property specified when injecting state into a grain.</param>
    /// <param name="configureOptions">The configuration delegate.</param>
    /// <returns>The silo builder.</returns>
    public static ISiloBuilder AddMemoryEventStorage(
        this ISiloBuilder builder,
        string name,
        Action<MemoryEventStorageOptions> configureOptions
    )
    {
        return builder.AddMemoryEventStorage(
            name,
            ob => ob.Configure(configureOptions)
        );
    }

    /// <summary>
    /// Configure the silo to use memory event storage
    /// </summary>
    /// <param name="builder">The silo builder</param>
    /// <param name="name">The name of the event storage provider. This must match the <c>ProviderName</c> property specified when injecting state into a grain.</param>
    /// <param name="configureOptions">The configuration delegate</param>
    /// <returns>The silo builder</returns>
    public static ISiloBuilder AddMemoryEventStorage(
        this ISiloBuilder builder,
        string name,
        Action<OptionsBuilder<MemoryEventStorageOptions>>? configureOptions = null
    )
    {
        return builder.ConfigureServices(services =>
        {
            configureOptions?.Invoke(services.AddOptions<MemoryEventStorageOptions>(name));
            services.ConfigureNamedOptionForLogging<MemoryEventStorageOptions>(name);

            const string defaultProviderName = EventStorageConstants.DEFAULT_EVENT_STORAGE_PROVIDER_NAME;
            if (string.Equals(name, defaultProviderName, StringComparison.Ordinal))
            {
                services.TryAddSingleton(
                    sp => sp.GetRequiredKeyedService<IEventStorage>(defaultProviderName)
                );
            }

            services.AddKeyedSingleton(name, MemoryEventStorageFactory.Create);
        });
    }
}