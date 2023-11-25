using System.Diagnostics;
using Orleans.Configuration;
using Orleans.Runtime;

namespace Orleans.EventSourcing.EventStorage.Memory;

[DebuggerDisplay("MemoryEventStore:{" + nameof(_name) + "}")]
public class MemoryEventStorage : IEventStorage
{
    private readonly string _name;
    private readonly MemoryEventStorageOptions _options;

    public MemoryEventStorage(string name, MemoryEventStorageOptions options)
    {
        _name = name;
        _options = options;
    }

    public IAsyncEnumerable<EventRecord<TEvent>> ReadEventsFromStorage<TEvent>(
        GrainId grainId,
        int version = 0,
        int maxCount = 2147483647
    ) where TEvent : class
    {
        throw new NotImplementedException();
    }

    public Task<bool> AppendEventsToStorage<TEvent>(
        GrainId grainId,
        IEnumerable<TEvent> events,
        int expectedVersion
    )
        where TEvent : class
    {
        throw new NotImplementedException();
    }
}