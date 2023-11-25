namespace Orleans.EventSourcing.EventStorage.Tests;

using static TestClusterSetup;

public class CounterGrainTests
{
    [Test]
    public void Counter_can_be_reset()
    {
        var counter = GrainFactory.GetGrain<ICounterGrain>(Guid.NewGuid());
        Assert.DoesNotThrowAsync(async () => await counter.Reset());
    }
}