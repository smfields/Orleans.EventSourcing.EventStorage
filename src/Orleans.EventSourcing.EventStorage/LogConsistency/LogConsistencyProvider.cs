using Orleans.Storage;

namespace Orleans.EventSourcing.EventStorage;

/// <summary>
/// A log-consistency provider that stores the entire log in event storage as a series of events.
/// </summary>
public class LogConsistencyProvider : ILogViewAdaptorFactory
{
    private readonly IServiceProvider _serviceProvider;

    public bool UsesStorageProvider => false;

    public LogConsistencyProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public ILogViewAdaptor<TView, TEntry> MakeLogViewAdaptor<TView, TEntry>(
        ILogViewAdaptorHost<TView, TEntry> hostGrain,
        TView initialstate,
        string grainTypeName,
        IGrainStorage grainStorage,
        ILogConsistencyProtocolServices services
    )
        where TView : class, new()
        where TEntry : class
    {
        var eventStorage = EventStorageHelpers.GetEventStorage(hostGrain.GetType(), _serviceProvider);
        return new LogViewAdaptor<TView, TEntry>(hostGrain, initialstate, services, eventStorage);
    }
}