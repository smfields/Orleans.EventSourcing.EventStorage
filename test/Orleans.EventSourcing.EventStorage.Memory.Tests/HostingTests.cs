using Microsoft.Extensions.DependencyInjection;
using Orleans.Runtime;

namespace Orleans.EventSourcing.EventStorage.Memory.Tests;

using static TestClusterSetup;

public class HostingTests
{
    [Test]
    public void Memory_storage_is_registered_as_default()
    {
        var silo = Cluster.Primary as TestingHost.InProcessSiloHandle;
        var eventStorage = silo!.SiloHost.Services.GetRequiredService<IEventStorage>();
        Assert.That(eventStorage, Is.TypeOf<MemoryEventStorage>());
    }

    [Test]
    public void Memory_storage_is_registered_by_name()
    {
        var silo = Cluster.Primary as TestingHost.InProcessSiloHandle;
        var eventStorage = silo!.SiloHost.Services.GetRequiredServiceByName<IEventStorage>("MemoryEventStorage");
        Assert.That(eventStorage, Is.TypeOf<MemoryEventStorage>());
    }
}