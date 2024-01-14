using System.Runtime.Serialization;
using Orleans.Runtime;

namespace Orleans.EventSourcing.EventStorage.Snapshots;

/// <summary>
/// Exception thrown whenever a grain call is attempted with a bad / missing snapshot storage provider configuration settings for that grain.
/// </summary>
[GenerateSerializer, Serializable]
public sealed class BadSnapshotStorageProviderConfigException : OrleansException
{
    public BadSnapshotStorageProviderConfigException()
    {
    }

    public BadSnapshotStorageProviderConfigException(string msg)
        : base(msg)
    {
    }

    public BadSnapshotStorageProviderConfigException(string msg, Exception exc)
        : base(msg, exc)
    {
    }

    private BadSnapshotStorageProviderConfigException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}