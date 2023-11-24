using Orleans.Runtime;

namespace Orleans.EventSourcing.EventStorage;

/// <summary>
/// Interface for storage that is able to persist events and retrieve event streams.
/// </summary>
public interface IEventStorage
{
    /// <summary>
    /// Read events from event stream.
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
    /// Append events to event stream.
    /// </summary>
    /// <param name="grainId">ID of the grain to which the events belong</param>
    /// <param name="events">Enumerable of events to append to the stream</param>
    /// <param name="expectedVersion">The expected current version of the stream</param>
    /// <typeparam name="TEvent">The grain event type</typeparam>
    /// <returns>Completion promise for the append operation</returns>
    Task AppendEventsToStorage<TEvent>(
        GrainId grainId,
        IEnumerable<TEvent> events,
        int expectedVersion
    ) where TEvent : class;
}