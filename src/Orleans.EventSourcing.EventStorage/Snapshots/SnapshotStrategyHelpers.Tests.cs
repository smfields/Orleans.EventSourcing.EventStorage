using Microsoft.Extensions.DependencyInjection;

namespace Orleans.EventSourcing.EventStorage.Snapshots;

public class SnapshotStrategyHelpersTests
{
    private class NoAttributeClass
    {
    }

    [SnapshotStrategy]
    private class DefaultAttributeClass
    {
    }

    [SnapshotStrategy(StrategyName = StrategyName)]
    private class NamedAttributeClass
    {
        public const string StrategyName = "NamedStrategy";
    }

    [Test]
    public void GetSnapshotStrategy_returns_default_strategy_when_class_has_no_attribute()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<ISnapshotStrategy>());
        var serviceProvider = services.BuildServiceProvider();

        var strategy = SnapshotStrategyHelpers.GetSnapshotStrategy(typeof(NoAttributeClass), serviceProvider);

        Assert.That(strategy, Is.Not.Null);
    }

    [Test]
    public void GetSnapshotStrategy_returns_default_storage_provider_when_class_has_default_attribute()
    {
        var services = new ServiceCollection();

        services.AddKeyedSingleton<IEventStorage>(
            EventStorageConstants.DEFAULT_EVENT_STORAGE_PROVIDER_NAME,
            (_, _) => Mock.Of<IEventStorage>()
        );
        var serviceProvider = services.BuildServiceProvider();

        var eventStorage = SnapshotStrategyHelpers.GetSnapshotStrategy(typeof(DefaultAttributeClass), serviceProvider);

        Assert.That(eventStorage, Is.Not.Null);
    }

    [Test]
    public void GetSnapshotStrategy_returns_named_storage_provider_when_class_has_named_attribute()
    {
        var services = new ServiceCollection();
        services.AddKeyedSingleton<IEventStorage>(
            NamedAttributeClass.ProviderName,
            (_, _) => Mock.Of<IEventStorage>()
        );
        var serviceProvider = services.BuildServiceProvider();

        var eventStorage = SnapshotStrategyHelpers.GetSnapshotStrategy(typeof(NamedAttributeClass), serviceProvider);

        Assert.That(eventStorage, Is.Not.Null);
    }

    [Test]
    public void GetSnapshotStrategy_throws_an_exception_if_no_matching_provider_can_be_found()
    {
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        Assert.Throws<BadEventStorageProviderConfigException>(
            () => SnapshotStrategyHelpers.GetSnapshotStrategy(typeof(NoAttributeClass), serviceProvider)
        );
    }
}