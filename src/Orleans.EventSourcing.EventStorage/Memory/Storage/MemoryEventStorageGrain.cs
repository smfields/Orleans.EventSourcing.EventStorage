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
    /// <typeparam name="TEvent">The grain event type</typeparam>
    /// <returns><see cref="IAsyncEnumerable{T}"/> containing the requested events</returns>
    IAsyncEnumerable<EventRecord<TEvent>> ReadEventsFromStorage<TEvent>(
        GrainId grainId,
        int version = 0,
        int maxCount = 2147483647
    ) where TEvent : class;

    /// <summary>
    /// Append events to an event stream.
    /// </summary>
    /// <param name="grainId">ID of the grain to which the events belong</param>
    /// <param name="events">Enumerable of events to append to the stream</param>
    /// <param name="expectedVersion">The expected current version of the stream</param>
    /// <typeparam name="TEvent">The grain event type</typeparam>
    /// <returns>Completion promise that returns true if the events were appended successfully, or false otherwise</returns>
    Task<bool> AppendEventsToStorage<TEvent>(
        GrainId grainId,
        IEnumerable<TEvent> events,
        int expectedVersion
    ) where TEvent : class;
}

internal class MemoryEventStorageGrain : IMemoryEventStorageGrain
{
    private readonly ILogger<MemoryEventStorageGrain> _logger;
    private readonly Dictionary<GrainId, dynamic> _store = new();

    public MemoryEventStorageGrain(ILogger<MemoryEventStorageGrain> logger)
    {
        _logger = logger;
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async IAsyncEnumerable<EventRecord<TEvent>> ReadEventsFromStorage<TEvent>(
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        GrainId grainId,
        int version = 0,
        int maxCount = 2147483647
    ) where TEvent : class
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("ReadEventsFromStorage for grain: {GrainStoreKey}", grainId);
        }

        var exists = _store.TryGetValue(grainId, out var entries);

        if (!exists || entries is not List<TEvent> events)
        {
            yield break;
        }

        for (var i = 0; i < events.Count; i++)
        {
            yield return new EventRecord<TEvent>(events[i], i);
        }
    }

    public Task<bool> AppendEventsToStorage<TEvent>(GrainId grainId, IEnumerable<TEvent> events, int expectedVersion)
        where TEvent : class
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
            entries = new List<TEvent>();
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