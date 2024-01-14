namespace Orleans.EventSourcing.EventStorage.Snapshots;

[AttributeUsage(AttributeTargets.Class)]
public class SnapshotStorageProviderAttribute : Attribute
{
    public string ProviderName { get; set; } = EventStorageConstants.DEFAULT_SNAPSHOT_STORAGE_PROVIDER_NAME;
}