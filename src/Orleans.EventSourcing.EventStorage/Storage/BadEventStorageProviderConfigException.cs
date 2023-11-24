using System.Runtime.Serialization;
using Orleans.Runtime;

namespace Orleans.EventSourcing.EventStorage;

/// <summary>
/// Exception thrown whenever a grain call is attempted with a bad / missing event storage provider configuration settings for that grain.
/// </summary>
[GenerateSerializer, Serializable]
public sealed class BadEventStorageProviderConfigException : OrleansException
{
    public BadEventStorageProviderConfigException()
    {
    }

    public BadEventStorageProviderConfigException(string msg)
        : base(msg)
    {
    }

    public BadEventStorageProviderConfigException(string msg, Exception exc)
        : base(msg, exc)
    {
    }

    private BadEventStorageProviderConfigException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}