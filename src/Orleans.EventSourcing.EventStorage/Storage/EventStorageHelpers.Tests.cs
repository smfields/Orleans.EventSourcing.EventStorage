using Microsoft.Extensions.DependencyInjection;

namespace Orleans.EventSourcing.EventStorage;

public class EventStorageHelpersTests
{
    private class NoAttributeClass
    {
    }

    [EventStorageProvider]
    private class DefaultAttributeClass
    {
    }

    [EventStorageProvider(ProviderName = ProviderName)]
    private class NamedAttributeClass
    {
        public const string ProviderName = "NamedStorageProvider";
    }

    [Test]
    public void GetEventStorage_returns_default_storage_provider_when_class_has_no_attribute()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IEventStorage>());
        var serviceProvider = services.BuildServiceProvider();

        var eventStorage = EventStorageHelpers.GetEventStorage(typeof(NoAttributeClass), serviceProvider);

        Assert.That(eventStorage, Is.Not.Null);
    }

    [Test]
    public void GetEventStorage_returns_default_storage_provider_when_class_has_default_attribute()
    {
        var services = new ServiceCollection();

        services.AddKeyedSingleton<IEventStorage>(
            EventStorageConstants.DEFAULT_EVENT_STORAGE_PROVIDER_NAME,
            (_, _) => Mock.Of<IEventStorage>()
        );
        var serviceProvider = services.BuildServiceProvider();

        var eventStorage = EventStorageHelpers.GetEventStorage(typeof(DefaultAttributeClass), serviceProvider);

        Assert.That(eventStorage, Is.Not.Null);
    }

    [Test]
    public void GetEventStorage_returns_named_storage_provider_when_class_has_named_attribute()
    {
        var services = new ServiceCollection();
        services.AddKeyedSingleton<IEventStorage>(
            NamedAttributeClass.ProviderName,
            (_, _) => Mock.Of<IEventStorage>()
        );
        var serviceProvider = services.BuildServiceProvider();

        var eventStorage = EventStorageHelpers.GetEventStorage(typeof(NamedAttributeClass), serviceProvider);

        Assert.That(eventStorage, Is.Not.Null);
    }
}