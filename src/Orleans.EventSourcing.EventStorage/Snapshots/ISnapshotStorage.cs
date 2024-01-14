using Orleans.Runtime;

namespace Orleans.EventSourcing.EventStorage.Snapshots;

/// <summary>
/// Interface for snapshot storage
/// </summary>
public interface ISnapshotStorage : ISnapshotReader, ISnapshotWriter;

/// <summary>
/// Interface for an object capable of reading the latest snapshot from storage
/// </summary>
public interface ISnapshotReader
{
    /// <summary>
    /// Loads the latest snapshot for a grain
    /// </summary>
    /// <param name="grainId">ID of the grain to which the snapshot belongs</param>
    /// <typeparam name="TState">Type of state stored in the snapshot</typeparam>
    /// <returns>Record of the latest snapshot take for the grain</returns>
    public Task<SnapshotRecord<TState>> ReadLatestSnapshot<TState>(GrainId grainId) where TState : class, new();
}

/// <summary>
/// Interface for an object capable of writing a snapshot to storage
/// </summary>
public interface ISnapshotWriter
{
    /// <summary>
    /// Saves a snapshot of the state for a grain
    /// </summary>
    /// <param name="grainId">ID of the grain to which the snapshot belongs</param>
    /// <param name="state">Snapshot of the state of the grain</param>
    /// <param name="version">The version of the state the snapshot represents</param>
    /// <typeparam name="TState">Type of state stored in the snapshot</typeparam>
    /// <returns>True if the snapshot was saved successfully, false otherwise.</returns>
    public Task<bool> SaveSnapshot<TState>(GrainId grainId, TState state, int version) where TState : class, new();
}