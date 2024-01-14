using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Orleans.EventSourcing.EventStorage.Snapshots;

public static class SnapshotStrategyHelpers
{
    public static ISnapshotStrategy GetSnapshotStrategy(Type grainType, IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(grainType);

        var attrs = grainType.GetCustomAttributes(typeof(SnapshotStrategyAttribute), true);
        var attr = attrs.Length > 0 ? (SnapshotStrategyAttribute)attrs[0] : null;
        var snapshotStrategy = attr != null
            ? services.GetKeyedService<ISnapshotStrategy>(attr.StrategyName)
            : services.GetService<ISnapshotStrategy>();

        if (snapshotStrategy == null)
        {
            ThrowMissingStrategyException(grainType, attr?.StrategyName);
        }

        return snapshotStrategy;
    }

    public static ISnapshotStorage GetSnapshotStorage(Type grainType, IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(grainType);

        var attrs = grainType.GetCustomAttributes(typeof(SnapshotStorageProviderAttribute), true);
        var attr = attrs.Length > 0 ? (SnapshotStorageProviderAttribute)attrs[0] : null;
        var snapshotStorageProvider = attr != null
            ? services.GetKeyedService<ISnapshotStorage>(attr.ProviderName)
            : services.GetService<ISnapshotStorage>();

        if (snapshotStorageProvider == null)
        {
            ThrowMissingStorageProviderException(grainType, attr?.ProviderName);
        }

        return snapshotStorageProvider;
    }

    [DoesNotReturn]
    private static void ThrowMissingStrategyException(Type grainType, string? name)
    {
        var grainTypeName = grainType.FullName;
        var errMsg = string.IsNullOrEmpty(name)
            ? $"No default snapshot strategy found loading grain type {grainTypeName}."
            : $"No snapshot strategy named \"{name}\" found loading grain type {grainTypeName}.";
        throw new BadSnapshotStrategyConfigException(errMsg);
    }

    [DoesNotReturn]
    private static void ThrowMissingStorageProviderException(Type grainType, string? name)
    {
        var grainTypeName = grainType.FullName;
        var errMsg = string.IsNullOrEmpty(name)
            ? $"No default snapshot storage provider found loading grain type {grainTypeName}."
            : $"No snapshot storage provider named \"{name}\" found loading grain type {grainTypeName}.";
        throw new BadSnapshotStorageProviderConfigException(errMsg);
    }
}