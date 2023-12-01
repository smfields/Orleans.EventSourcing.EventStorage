using Microsoft.Extensions.Logging;
using Orleans.Runtime;

namespace Orleans.EventSourcing.EventStorage.Memory;

public interface IMemoryEventStorageGrain : IGrainWithIntegerKey
{
    /// <summary>
    /// Read events from an event stream.
    /// </summary>
    /// <param name="grainId">ID of the grain to which the events belong</param>
    /// <param name="version">The version to start reading from</param>
    /// <param name="maxCount">The number of events to read</param>
    /// <returns><see cref="IAsyncEnumerable{T}"/> containing the requested events</returns>
    IAsyncEnumerable<EventRecord<ReadOnlyMemory<byte>>> ReadEventsFromStorage(
        GrainId grainId,
        int version = 0,
        int maxCount = 2147483647
    );

    /// <summary>
    /// Append events to an event stream.
    /// </summary>
    /// <param name="grainId">ID of the grain to which the events belong</param>
    /// <param name="events">Enumerable of events to append to the stream</param>
    /// <param name="expectedVersion">The expected current version of the stream</param>
    /// <returns>Completion promise that returns true if the events were appended successfully, or false otherwise</returns>
    Task<bool> AppendEventsToStorage(
        GrainId grainId,
        IEnumerable<ReadOnlyMemory<byte>> events,
        int expectedVersion
    );
}

internal class MemoryEventStorageGrain : IMemoryEventStorageGrain
{
    private readonly ILogger<MemoryEventStorageGrain> _logger;
    private readonly Dictionary<GrainId, List<ReadOnlyMemory<byte>>> _store = new();

    public MemoryEventStorageGrain(ILogger<MemoryEventStorageGrain> logger)
    {
        _logger = logger;
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async IAsyncEnumerable<EventRecord<ReadOnlyMemory<byte>>> ReadEventsFromStorage(
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        GrainId grainId,
        int version = 0,
        int maxCount = 2147483647
    )
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("ReadEventsFromStorage for grain: {GrainStoreKey}", grainId);
        }

        var exists = _store.TryGetValue(grainId, out var entries);

        if (!exists || entries is null)
        {
            yield break;
        }

        for (var i = 0; i < entries.Count; i++)
        {
            yield return new EventRecord<ReadOnlyMemory<byte>>(entries[i], i);
        }
    }

    public Task<bool> AppendEventsToStorage(
        GrainId grainId,
        IEnumerable<ReadOnlyMemory<byte>> events,
        int expectedVersion
    )
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug(
                "AppendEventsToStorage for grain: {GrainStoreKey} expectedVersion: {ExpectedVersion}",
                grainId,
                expectedVersion
            );
        }

        if (!_store.TryGetValue(grainId, out var entries))
        {
            entries = new List<ReadOnlyMemory<byte>>();
            _store[grainId] = entries;
        }

        if (entries.Count != expectedVersion)
        {
            return Task.FromResult(false);
        }

        entries.AddRange(events);

        return Task.FromResult(true);
    }
}