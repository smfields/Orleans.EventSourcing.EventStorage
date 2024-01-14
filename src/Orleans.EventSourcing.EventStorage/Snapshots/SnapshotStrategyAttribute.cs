namespace Orleans.EventSourcing.EventStorage.Snapshots;

[AttributeUsage(AttributeTargets.Class)]
public class SnapshotStrategyAttribute : Attribute
{
    public string StrategyName { get; set; } = EventStorageConstants.DEFAULT_SNAPSHOT_STRATEGY_NAME;
}