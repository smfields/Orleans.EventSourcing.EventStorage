﻿using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Storage;

namespace Orleans.EventSourcing.EventStorage.Memory;

/// <summary>
/// Simple in-memory implementation of an event storage provider. Events are persisted in-memory using grains.
/// </summary>
/// <remarks>
/// This provider is ONLY intended for simple in-memory Development / Unit Test scenarios.
/// This class should NOT be used in Production environment, 
///  because [by-design] it does not provide any resilience 
///  or long-term persistence capabilities.
/// </remarks>
[DebuggerDisplay("MemoryEventStore:{" + nameof(_name) + "}")]
public class MemoryEventStorage : IEventStorage
{
    private readonly Lazy<IMemoryEventStorageGrain>[] _storageGrains;
    private readonly string _name;
    private readonly MemoryEventStorageOptions _options;
    private readonly ILogger<MemoryEventStorage> _logger;
    private readonly IGrainStorageSerializer _storageSerializer;

    public MemoryEventStorage(
        string name,
        MemoryEventStorageOptions options,
        ILogger<MemoryEventStorage> logger,
        IGrainFactory grainFactory,
        IGrainStorageSerializer defaultGrainStorageSerializer
    )
    {
        _name = name;
        _options = options;
        _logger = logger;
        _storageGrains = InitializeGrainReferences(grainFactory);
        _storageSerializer = options.GrainStorageSerializer ?? defaultGrainStorageSerializer;
    }

    private Lazy<IMemoryEventStorageGrain>[] InitializeGrainReferences(IGrainFactory grainFactory)
    {
        _logger.LogInformation(
            "Init: Name={Name} NumStorageGrains={NumStorageGrains}",
            _name,
            _options.NumStorageGrains
        );

        var storageGrains = new Lazy<IMemoryEventStorageGrain>[_options.NumStorageGrains];
        for (var i = 0; i < storageGrains.Length; i++)
        {
            var idx = i;
            storageGrains[idx] = new Lazy<IMemoryEventStorageGrain>(
                () => grainFactory.GetGrain<IMemoryEventStorageGrain>(idx)
            );
        }

        return storageGrains;
    }

    public async IAsyncEnumerable<EventRecord<TEvent>> ReadEventsFromStorage<TEvent>(
        GrainId grainId,
        int version = 0,
        int maxCount = 2147483647
    ) where TEvent : class
    {
        if (_logger.IsEnabled(LogLevel.Trace)) _logger.LogTrace("Read Key={Key}", grainId);
        var storageGrain = GetStorageGrain(grainId);
        var storedEvents = storageGrain.ReadEventsFromStorage(grainId, version, maxCount);

        await foreach (var entry in storedEvents)
        {
            var eventData = ConvertFromStorageFormat<TEvent>(entry.Data);
            yield return new EventRecord<TEvent>(eventData, entry.Version);
        }
    }

    public Task<bool> AppendEventsToStorage<TEvent>(
        GrainId grainId,
        IEnumerable<TEvent> events,
        int expectedVersion
    )
        where TEvent : class
    {
        if (_logger.IsEnabled(LogLevel.Trace)) _logger.LogTrace("Append Key={Key}", grainId);

        var storageGrain = GetStorageGrain(grainId);
        return storageGrain.AppendEventsToStorage(
            grainId,
            events.Select(ConvertToStorageFormat).ToList(),
            expectedVersion
        );
    }

    private IMemoryEventStorageGrain GetStorageGrain(GrainId id)
    {
        var idx = (uint)id.GetHashCode() % (uint)_storageGrains.Length;
        return _storageGrains[idx].Value;
    }

    /// <summary>
    /// Deserialize from binary data
    /// </summary>
    /// <param name="data">The serialized stored data</param>
    private T ConvertFromStorageFormat<T>(ReadOnlyMemory<byte> data)
    {
        try
        {
            return _storageSerializer.Deserialize<T>(data);
        }
        catch (Exception exc)
        {
            var sb = new StringBuilder();
            if (data.ToArray().Length > 0)
            {
                sb.Append($"Unable to convert from storage format GrainStateEntity.Data={data}");
            }

            _logger.LogError(exc, "{Message}", sb.ToString());
            throw new AggregateException(sb.ToString(), exc);
        }
    }

    /// <summary>
    /// Serialize to the storage format.
    /// </summary>
    /// <param name="grainEvent">The grain event data to be serialized</param>
    /// <remarks>
    /// See:
    /// http://msdn.microsoft.com/en-us/library/system.web.script.serialization.javascriptserializer.aspx
    /// for more on the JSON serializer.
    /// </remarks>
    private ReadOnlyMemory<byte> ConvertToStorageFormat<T>(T grainEvent)
    {
        // Convert to binary format
        return _storageSerializer.Serialize(grainEvent);
    }
}