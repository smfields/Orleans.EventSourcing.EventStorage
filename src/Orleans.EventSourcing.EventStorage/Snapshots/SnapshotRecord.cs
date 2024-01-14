namespace Orleans.EventSourcing.EventStorage.Snapshots;

/// <summary>
/// Record of a snapshot
/// </summary>
/// <param name="State">State captured in the snapshot</param>
/// <param name="Version">Version of the state the snapshot represents</param>
/// <typeparam name="TState">Type of state</typeparam>
public record SnapshotRecord<TState>(TState State, int Version);