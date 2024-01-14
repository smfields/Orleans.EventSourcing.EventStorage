using Microsoft.Extensions.DependencyInjection;
using Orleans.EventSourcing;
using Orleans.EventSourcing.EventStorage;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.TestingHost;

// ReSharper disable once CheckNamespace
namespace Orleans.Hosting;

public class EventStorageSiloBuilderExtensionsTests
{
    private TestCluster Cluster { get; set; } = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        var builder = new TestClusterBuilder();

        builder.AddSiloBuilderConfigurator<TestSiloConfigurator>();

        Cluster = builder.Build();
        await Cluster.DeployAsync();
    }

    [Test]
    public void EventStorage_provider_can_be_registered_as_default()
    {
        var silo = Cluster.Primary as InProcessSiloHandle;

        // Retrieve default (non-keyed) service
        var provider = silo!.SiloHost.Services.GetRequiredService<ILogViewAdaptorFactory>();
        Assert.That(provider, Is.TypeOf<LogConsistencyProvider>());

        // Retrieve default (keyed) service
        provider = silo.SiloHost.Services.GetKeyedService<ILogViewAdaptorFactory>(
            ProviderConstants.DEFAULT_LOG_CONSISTENCY_PROVIDER_NAME
        );
        Assert.That(provider, Is.TypeOf<LogConsistencyProvider>());
    }

    [Test]
    public void EventStorage_provider_can_be_registered_by_name()
    {
        var silo = Cluster.Primary as InProcessSiloHandle;
        var provider = silo!.SiloHost.Services.GetRequiredKeyedService<ILogViewAdaptorFactory>("EventStorage");
        Assert.That(provider, Is.TypeOf<LogConsistencyProvider>());
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await Cluster.StopAllSilosAsync();
    }

    private class TestSiloConfigurator : ISiloConfigurator
    {
        public void Configure(ISiloBuilder siloBuilder)
        {
            siloBuilder.AddEventStorageBasedLogConsistencyProviderAsDefault();
            siloBuilder.AddEventStorageBasedLogConsistencyProvider("EventStorage");
        }
    }
}