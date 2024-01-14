using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Orleans.EventSourcing.Common;
using Orleans.EventSourcing.EventStorage.Snapshots;

namespace Orleans.EventSourcing.EventStorage;

/// <summary>
/// A log view adaptor that wraps around an event stream and stores the log as a series of events.
/// </summary>
/// <typeparam name="TLogView">Type of log view</typeparam>
/// <typeparam name="TLogEntry">Type of log entry</typeparam>
internal class LogViewAdaptor<TLogView, TLogEntry> :
    PrimaryBasedLogViewAdaptor<TLogView, TLogEntry, SubmissionEntry<TLogEntry>>
    where TLogView : class, new()
    where TLogEntry : class
{
    private readonly ISnapshotStrategy _snapshotStrategy;
    private readonly IEventStorage _eventStorage;
    private TLogView _cached = default!;
    private int _version;

    public LogViewAdaptor(
        ILogViewAdaptorHost<TLogView, TLogEntry> host,
        TLogView initialState,
        ILogConsistencyProtocolServices services,
        ISnapshotStrategy snapshotStrategy,
        IEventStorage eventStorage
    )
        : base(host, initialState, services)
    {
        _snapshotStrategy = snapshotStrategy;
        _eventStorage = eventStorage;
    }

    /// <inheritdoc />
    protected override TLogView LastConfirmedView() => _cached;

    /// <inheritdoc />
    protected override int GetConfirmedVersion() => _version;

    /// <inheritdoc />
    protected override void InitializeConfirmedView(TLogView initialstate)
    {
        _cached = initialstate;
        _version = 0;
    }

    /// <inheritdoc />
    protected override SubmissionEntry<TLogEntry> MakeSubmissionEntry(TLogEntry entry)
    {
        return new SubmissionEntry<TLogEntry>
        {
            Entry = entry
        };
    }

    /// <inheritdoc />
    protected override async Task ReadAsync()
    {
        enter_operation(nameof(ReadAsync));

        while (true)
        {
            InitializeConfirmedView(new TLogView());

            try
            {
                // Load the latest snapshot.
                // If there is no snapshot then we'll start with the default object and the version will be 0.
                await TryLoadFromSnapshot();

                // Read all events from the latest snapshot to the end of the stream for this grain
                var entries = _eventStorage.ReadEventsFromStorage<TLogEntry>(
                    Services.GrainId,
                    _version
                );

                // Apply all events in order
                var transitionSuccessful = true;
                await foreach (var entry in entries)
                {
                    transitionSuccessful = ApplyEntry(entry.Data, entry.Version);

                    if (!transitionSuccessful)
                        // Failed to apply event
                        break;
                }

                if (transitionSuccessful)
                {
                    // Successfully read and applied events
                    Services.Log(LogLevel.Debug, "read success v{0}", _version);
                    LastPrimaryIssue.Resolve(Host, Services);
                    break;
                }
            }
            catch (Exception ex)
            {
                LastPrimaryIssue.Record(new ReadFromStorageFailed { Exception = ex }, Host, Services);
            }

            // Failed - Reset confirmed view and try again
            Services.Log(LogLevel.Debug, "read failed {0}", LastPrimaryIssue);
            await LastPrimaryIssue.DelayBeforeRetry();
        }

        exit_operation(nameof(ReadAsync));
    }

    /// <inheritdoc />
    protected override async Task<int> WriteAsync()
    {
        enter_operation(nameof(WriteAsync));

        // Gather update batch
        var updateEntries = GetCurrentBatchOfUpdates()
            .Select(submissionEntry => submissionEntry.Entry)
            .ToList();

        var writeSuccessful = false;
        var transitionSuccessful = true;

        // Try to write event update batch to event stream
        try
        {
            writeSuccessful = await _eventStorage.AppendEventsToStorage(
                Services.GrainId,
                updateEntries,
                _version
            );

            LastPrimaryIssue.Resolve(Host, Services);
        }
        catch (Exception ex)
        {
            ex = UnwrapTransportException(ex);

            LastPrimaryIssue.Record(new UpdatePrimaryFailed { Exception = ex }, Host, Services);
        }

        // Notify snapshot strategy of all appended events in case it wants to create a new snapshot
        foreach (var (logEntry, version) in updateEntries.Select((entry, i) => (entry, _version + 1 + i)))
        {
            await _snapshotStrategy.OnEventAppended(Services.GrainId, logEntry, version);
        }

        // Now that the events have been written to the event stream we can apply them locally
        if (writeSuccessful)
        {
            foreach (var logEntry in updateEntries)
            {
                transitionSuccessful = ApplyEntry(logEntry, _version + 1);

                if (!transitionSuccessful)
                    // Failed to apply event
                    break;
            }
        }

        // If we failed to write to the stream or to apply the events locally, reset the entire view by reloading from storage
        if (!writeSuccessful || !transitionSuccessful)
        {
            Services.Log(LogLevel.Debug, "{0} failed {1}", writeSuccessful ? "transitions" : "write", LastPrimaryIssue);
            await ReadAsync();
        }

        exit_operation(nameof(WriteAsync));

        return writeSuccessful ? updateEntries.Count : 0;
    }

    /// <inheritdoc />
    public override async Task<IReadOnlyList<TLogEntry>> RetrieveLogSegment(int fromVersion, int length)
    {
        var events = await _eventStorage
            .ReadEventsFromStorage<TLogEntry>(Services.GrainId, fromVersion, length)
            .Select(x => x.Data)
            .ToListAsync();

        return events.AsReadOnly();
    }

    /// <summary>
    /// Applies a single event to the local cached view
    /// </summary>
    /// <param name="entry">Entry event to apply</param>
    /// <param name="version">New version</param>
    /// <returns>True if the update was applied successfully, false otherwise.</returns>
    private bool ApplyEntry(TLogEntry entry, int version)
    {
        _version = version;

        try
        {
            Host.UpdateView(_cached, entry);
            return true;
        }
        catch (Exception ex)
        {
            Services.CaughtUserCodeException("UpdateView", nameof(ReadAsync), ex);
        }

        return false;
    }

    private async Task TryLoadFromSnapshot()
    {
        var snapshot = await _snapshotStrategy.Reader.ReadLatestSnapshot<TLogView>(Services.GrainId);
        _cached = snapshot.State;
        _version = snapshot.Version;
    }

    private static Exception UnwrapTransportException(Exception exception)
    {
        if (exception is ProtocolTransportException { InnerException: not null } transportException)
            exception = transportException.InnerException!;

        return exception;
    }

#if DEBUG
    private readonly Dictionary<string, bool> _operationInProgress = new();
#endif

    [Conditional("DEBUG")]
    private void enter_operation(string name)
    {
#if DEBUG
        Services.Log(LogLevel.Trace, "/-- enter {0}", name);
        Debug.Assert(!_operationInProgress.GetValueOrDefault(name));
        _operationInProgress[name] = true;
#endif
    }

    [Conditional("DEBUG")]
    private void exit_operation(string name)
    {
#if DEBUG
        Services.Log(LogLevel.Trace, "\\-- exit {0}", name);
        Debug.Assert(_operationInProgress.GetValueOrDefault(name));
        _operationInProgress[name] = false;
#endif
    }

    [GenerateSerializer]
    [Serializable]
    public sealed class UpdatePrimaryFailed : PrimaryOperationFailed
    {
        public override string ToString()
        {
            return $"update primary failed: caught {Exception.GetType().Name}: {Exception.Message}";
        }
    }

    [GenerateSerializer]
    [Serializable]
    public sealed class ReadFromStorageFailed : PrimaryOperationFailed
    {
        public override string ToString()
        {
            return $"read from primary failed: caught {Exception.GetType().Name}: {Exception.Message}";
        }
    }
}