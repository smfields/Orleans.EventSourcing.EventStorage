using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Orleans.EventSourcing.EventStorage;

public static class EventStorageHelpers
{
    public static IEventStorage GetEventStorage(Type grainType, IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(grainType);

        var attrs = grainType.GetCustomAttributes(typeof(EventStorageProviderAttribute), true);
        var attr = attrs.Length > 0 ? (EventStorageProviderAttribute)attrs[0] : null;
        var eventProvider = attr != null
            ? services.GetKeyedService<IEventStorage>(attr.ProviderName)
            : services.GetService<IEventStorage>();

        if (eventProvider == null)
        {
            ThrowMissingProviderException(grainType, attr?.ProviderName);
        }

        return eventProvider;
    }

    [DoesNotReturn]
    private static void ThrowMissingProviderException(Type grainType, string? name)
    {
        var grainTypeName = grainType.FullName;
        var errMsg = string.IsNullOrEmpty(name)
            ? $"No default event storage provider found loading grain type {grainTypeName}."
            : $"No event storage provider named \"{name}\" found loading grain type {grainTypeName}.";
        throw new BadEventStorageProviderConfigException(errMsg);
    }
}