using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Orleans.Serialization;

namespace Orleans.EventSourcing.EventStorage;

/// <summary>
/// Functionality for use by log view adaptors that run distributed protocols.
/// This class allows access to these services to providers that cannot see runtime-internals.
/// It also stores grain-specific information like the grain reference, and caches
/// </summary>
public class ProtocolServices : ILogConsistencyProtocolServices
{
    private readonly ILogger _log;
    private readonly DeepCopier _deepCopier;
    private readonly IGrainContext _grainContext; // links to the grain that owns this service object

    public ProtocolServices(
        IGrainContext grainContext,
        ILoggerFactory loggerFactory,
        DeepCopier deepCopier,
        ILocalSiloDetails siloDetails)
    {
        _grainContext = grainContext;
        _log = loggerFactory.CreateLogger<ProtocolServices>();
        _deepCopier = deepCopier;
        MyClusterId = siloDetails.ClusterId;
    }

    /// <inheritdoc />
    public GrainId GrainId => _grainContext.GrainId;

    /// <inheritdoc />
    public string MyClusterId { get; }

    /// <inheritdoc />
    public T DeepCopy<T>(T value) => _deepCopier.Copy(value);

    /// <inheritdoc />
    public void ProtocolError(string message, bool throwException)
    {
        _log.LogError(
            (int)(throwException
                ? ErrorCode.LogConsistency_ProtocolFatalError
                : ErrorCode.LogConsistency_ProtocolError),
            "{GrainId} Protocol Error: {Message}",
            _grainContext.GrainId,
            message
        );

        if (!throwException)
            return;

        throw new OrleansException($"{message} (grain={_grainContext.GrainId}, cluster={MyClusterId})");
    }

    /// <inheritdoc />
    public void CaughtException(string where, Exception e)
    {
        _log.LogError(
            (int)ErrorCode.LogConsistency_CaughtException,
            e,
            "{GrainId} exception caught at {Location}",
            _grainContext.GrainId,
            where
        );
    }

    /// <inheritdoc />
    public void CaughtUserCodeException(string callback, string where, Exception e)
    {
        _log.LogWarning(
            (int)ErrorCode.LogConsistency_UserCodeException,
            e,
            "{GrainId} exception caught in user code for {Callback}, called from {Location}",
            _grainContext.GrainId,
            callback,
            where
        );
    }

    /// <inheritdoc />
    public void Log(LogLevel level, string format, params object[] args)
    {
        if (!_log.IsEnabled(level))
            return;

        var msg = $"{_grainContext.GrainId} {string.Format(format, args)}";
        _log.Log(level, 0, msg, null, (m, exc) => $"{m}");
    }
}