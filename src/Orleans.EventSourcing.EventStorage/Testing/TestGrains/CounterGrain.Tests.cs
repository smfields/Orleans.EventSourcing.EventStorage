using Orleans.TestingHost;

namespace Orleans.EventSourcing.EventStorage.Testing.TestGrains;

public class CounterGrainTests
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
    public async Task Counters_initial_value_is_0()
    {
        var counter = GrainFactory.GetGrain<ICounterGrain>(Guid.NewGuid());

        var currentValue = await counter.GetCurrentValue();

        Assert.That(currentValue, Is.EqualTo(0));
    }

    [Test]
    public void Counter_value_can_be_retrieved()
    {
        var counter = GrainFactory.GetGrain<ICounterGrain>(Guid.NewGuid());

        Assert.DoesNotThrowAsync(async () => await counter.GetCurrentValue());
    }

    [Test]
    public void Counter_can_be_reset()
    {
        var counter = GrainFactory.GetGrain<ICounterGrain>(Guid.NewGuid());

        Assert.DoesNotThrowAsync(async () => await counter.Reset());
    }

    [Test]
    public void Counter_can_be_incremented()
    {
        var counter = GrainFactory.GetGrain<ICounterGrain>(Guid.NewGuid());

        Assert.DoesNotThrowAsync(async () => await counter.Increment(1));
    }

    [Test]
    public async Task Incrementing_increases_value_by_the_specified_amount()
    {
        const int startingValue = 0;
        const int incrementAmount = 15;
        var counter = GrainFactory.GetGrain<ICounterGrain>(Guid.NewGuid());

        await counter.Increment(incrementAmount);
        await counter.ConfirmEvents();

        var currentValue = await counter.GetCurrentValue();
        Assert.That(currentValue, Is.EqualTo(startingValue + incrementAmount));
    }

    [Test]
    public void Counter_can_be_decremented()
    {
        var counter = GrainFactory.GetGrain<ICounterGrain>(Guid.NewGuid());

        Assert.DoesNotThrowAsync(async () => await counter.Decrement(1));
    }

    [Test]
    public async Task Decrementing_decreases_value_by_the_specified_amount()
    {
        const int startingValue = 0;
        const int decrementAmount = 15;
        var counter = GrainFactory.GetGrain<ICounterGrain>(Guid.NewGuid());

        await counter.Decrement(decrementAmount);
        await counter.ConfirmEvents();

        var currentValue = await counter.GetCurrentValue();
        Assert.That(currentValue, Is.EqualTo(startingValue - decrementAmount));
    }

    [Test]
    public async Task Counter_can_be_reset_to_the_minimum_value()
    {
        var counter = GrainFactory.GetGrain<ICounterGrain>(Guid.NewGuid());

        await counter.Reset(int.MinValue);
        await counter.ConfirmEvents();

        var currentValue = await counter.GetCurrentValue();
        Assert.That(currentValue, Is.EqualTo(int.MinValue));
    }

    [Test]
    public async Task Counter_can_be_reset_to_the_maximum_value()
    {
        var counter = GrainFactory.GetGrain<ICounterGrain>(Guid.NewGuid());

        await counter.Reset(int.MaxValue);
        await counter.ConfirmEvents();

        var currentValue = await counter.GetCurrentValue();
        Assert.That(currentValue, Is.EqualTo(int.MaxValue));
    }

    [Test]
    public async Task Decrement_will_throw_an_exception_if_value_would_underflow()
    {
        var counter = GrainFactory.GetGrain<ICounterGrain>(Guid.NewGuid());
        await counter.Reset(int.MinValue);
        await counter.ConfirmEvents();

        Assert.ThrowsAsync<OverflowException>(async () => await counter.Decrement(1));
    }

    [Test]
    public async Task Increment_will_throw_an_exception_if_value_would_overflow()
    {
        var counter = GrainFactory.GetGrain<ICounterGrain>(Guid.NewGuid());
        await counter.Reset(int.MaxValue);
        await counter.ConfirmEvents();

        Assert.ThrowsAsync<OverflowException>(async () => await counter.Increment(1));
    }

    [Test]
    public void Incrementing_by_a_long_value_will_throw_an_exception()
    {
        var counter = GrainFactory.GetGrain<ICounterGrain>(Guid.NewGuid());

        Assert.ThrowsAsync<OverflowException>(async () => await counter.Increment(uint.MaxValue));
    }

    [Test]
    public async Task Counter_state_can_be_reconstructed_from_stored_events()
    {
        var counterGrain = GrainFactory.GetGrain<ICounterGrain>(Guid.NewGuid());
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