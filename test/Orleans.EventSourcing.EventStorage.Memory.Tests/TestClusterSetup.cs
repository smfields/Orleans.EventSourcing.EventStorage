using Orleans.TestingHost;

namespace Orleans.EventSourcing.EventStorage.Memory.Tests;

[SetUpFixture]
public static class TestClusterSetup
{
    public static TestCluster Cluster { get; private set; } = null!;
    public static IGrainFactory GrainFactory => Cluster.GrainFactory;


    [OneTimeSetUp]
    public static async Task OneTimeSetup()
    {
        var builder = new TestClusterBuilder();

        builder.AddSiloBuilderConfigurator<TestSiloConfigurator>();

        Cluster = builder.Build();
        await Cluster.DeployAsync();
    }

    [OneTimeTearDown]
    public static async Task OneTimeTearDown()
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