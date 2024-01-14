using System.Runtime.Serialization;
using Orleans.Runtime;

namespace Orleans.EventSourcing.EventStorage.Snapshots;

/// <summary>
/// Exception thrown whenever a grain call is attempted with a bad / missing snapshot strategy configuration settings for that grain.
/// </summary>
[GenerateSerializer, Serializable]
public sealed class BadSnapshotStrategyConfigException : OrleansException
{
    public BadSnapshotStrategyConfigException()
    {
    }

    public BadSnapshotStrategyConfigException(string msg)
        : base(msg)
    {
    }

    public BadSnapshotStrategyConfigException(string msg, Exception exc)
        : base(msg, exc)
    {
    }

    private BadSnapshotStrategyConfigException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}