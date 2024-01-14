using Orleans.Runtime;

namespace Orleans.EventSourcing.EventStorage.Snapshots;

/// <summary>
/// Strategy used to control how snapshots are created
/// </summary>
public interface ISnapshotStrategy
{
    /// <summary>
    /// Snapshot reader
    /// </summary>
    public ISnapshotReader Reader { get; }

    /// <summary>
    /// Allows the snapshot strategy to respond to a new event being appended to the grain. This is where the strategy
    /// can implement logic to create a new snapshot.
    /// </summary>
    /// <param name="grainId">ID of the grain where the event was appended</param>
    /// <param name="newEvent">The event that was appended to the grain</param>
    /// <param name="version">Updated version of the grain</param>
    /// <typeparam name="TEvent">Type of event appended to the grain</typeparam>
    /// <returns>Task that resolves once the strategy has finished responding</returns>
    public Task OnEventAppended<TEvent>(
        GrainId grainId,
        TEvent newEvent,
        int version
    ) where TEvent : class;
}