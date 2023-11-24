namespace Orleans.EventSourcing.EventStorage;

[AttributeUsage(AttributeTargets.Class)]
public class EventStorageProviderAttribute : Attribute
{
    public string ProviderName { get; set; } = EventStorageConstants.DEFAULT_EVENT_STORAGE_PROVIDER_NAME;
}