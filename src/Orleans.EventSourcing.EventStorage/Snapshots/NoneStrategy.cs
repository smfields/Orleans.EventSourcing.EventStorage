using Orleans.Runtime;

namespace Orleans.EventSourcing.EventStorage.Snapshots;

public class NoneStrategy(ISnapshotReader reader) : ISnapshotStrategy
{
    public ISnapshotReader Reader { get; } = reader;

    public Task OnEventAppended<TEvent>(GrainId grainId, TEvent newEvent, int version) where TEvent : class
    {
        return Task.CompletedTask;
    }
}