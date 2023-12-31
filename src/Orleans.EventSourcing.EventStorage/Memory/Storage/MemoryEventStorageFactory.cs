﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;

namespace Orleans.EventSourcing.EventStorage.Memory;

/// <summary>
/// Factory for <see cref="MemoryEventStorage"/>
/// </summary>
public static class MemoryEventStorageFactory
{
    public static IEventStorage Create(IServiceProvider services, object? key)
    {
        var name = (string)key!;

        return ActivatorUtilities.CreateInstance<MemoryEventStorage>(
            services,
            services.GetRequiredService<IOptionsMonitor<MemoryEventStorageOptions>>().Get(name),
            name
        );
    }
}