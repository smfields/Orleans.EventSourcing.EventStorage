using Microsoft.Extensions.DependencyInjection;
using Orleans.EventSourcing.EventStorage;
using Orleans.EventSourcing.EventStorage.Memory;
using Orleans.Runtime;
using Orleans.TestingHost;

// ReSharper disable once CheckNamespace
namespace Orleans.Hosting;

public class MemoryEventStorageSiloBuilderExtensionsTests
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
    public void Memory_storage_can_be_registered_as_default()
    {
        var silo = Cluster.Primary as InProcessSiloHandle;
        var eventStorage = silo!.SiloHost.Services.GetRequiredService<IEventStorage>();
        Assert.That(eventStorage, Is.TypeOf<MemoryEventStorage>());
    }

    [Test]
    public void Memory_storage_can_be_registered_by_name()
    {
        var silo = Cluster.Primary as InProcessSiloHandle;
        var eventStorage = silo!.SiloHost.Services.GetRequiredKeyedService<IEventStorage>("MemoryEventStorage");
        Assert.That(eventStorage, Is.TypeOf<MemoryEventStorage>());
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
            siloBuilder.AddMemoryEventStorage("MemoryEventStorage");
            siloBuilder.AddMemoryEventStorageAsDefault();
        }
    }
}