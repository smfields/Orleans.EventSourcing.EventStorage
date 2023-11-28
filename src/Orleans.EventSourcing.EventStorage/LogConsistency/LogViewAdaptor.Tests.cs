using Orleans.EventSourcing.EventStorage.Testing.TestGrains;
using Orleans.TestingHost;

namespace Orleans.EventSourcing.EventStorage;

public class LogViewAdaptorTests
{
    private TestCluster Cluster { get; set; } = null!;
    private IGrainFactory GrainFactory => Cluster.GrainFactory;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        var builder = new TestClusterBuilder();

        builder.AddSiloBuilderConfigurator<TestSiloConfigurator>();

        Cluster = builder.Build();
        await Cluster.DeployAsync();
    }

    [Test]
    public async Task ReadAsync_can_reconstruct_an_existing_grain_from_stored_events()
    {
        var counterId = Guid.NewGuid();
        var counterGrain = GrainFactory.GetGrain<ICounterGrain>(counterId);
        await counterGrain.Reset(1000); // 1000
        await counterGrain.Increment(10); // 1010
        await counterGrain.Increment(15); // 1025
        await counterGrain.Increment(20); // 1045
        await counterGrain.Increment(25); // 1070
        await counterGrain.Decrement(30); // 1040
        await counterGrain.Decrement(35); // 1005
        await counterGrain.ConfirmEvents();
        await counterGrain.Deactivate();

        var currentValue = await counterGrain.GetCurrentValue();

        Assert.That(currentValue, Is.EqualTo(1005));
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
            siloBuilder
                .AddEventStorageBasedLogConsistencyProviderAsDefault()
                .AddMemoryEventStorageAsDefault();
        }
    }
}