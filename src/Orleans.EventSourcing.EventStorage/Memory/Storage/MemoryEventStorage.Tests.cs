using Microsoft.Extensions.DependencyInjection;
using Orleans.Runtime;
using Orleans.TestingHost;

namespace Orleans.EventSourcing.EventStorage.Memory;

public class MemoryEventStorageTests
{
    private TestCluster Cluster { get; set; } = null!;

    private MemoryEventStorage EventStorage
    {
        get
        {
            var silo = Cluster.Primary as InProcessSiloHandle;
            var memoryStorage = silo!.SiloHost.Services.GetRequiredService<IEventStorage>();
            return (MemoryEventStorage)memoryStorage;
        }
    }

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        var builder = new TestClusterBuilder();

        builder.AddSiloBuilderConfigurator<TestSiloConfigurator>();

        Cluster = builder.Build();
        await Cluster.DeployAsync();
    }

    [Test]
    public async Task Events_can_be_stored_and_retrieved()
    {
        var grainId = GenerateGrainId();

        var sampleEvent = new SampleEvent(100);
        await EventStorage.AppendEventsToStorage(grainId, new[] { sampleEvent }, 0);
        var eventStream = EventStorage.ReadEventsFromStorage<SampleEvent>(grainId, 0, 1);
        var eventList = await eventStream.ToListAsync();

        Assert.That(eventList.First().Data, Is.EqualTo(sampleEvent));
    }

    [Test]
    public async Task Retrieved_events_have_same_type_as_stored_events()
    {
        var grainId = GenerateGrainId();

        await EventStorage.AppendEventsToStorage(grainId, new[] { new SampleEvent() }, 0);
        var eventStream = EventStorage.ReadEventsFromStorage<object>(grainId, 0, 1);
        var eventList = await eventStream.ToListAsync();

        Assert.That(eventList.First().Data, Is.TypeOf<SampleEvent>());
    }

    [Test]
    public async Task Events_are_not_appended_if_the_expected_version_does_not_match()
    {
        var grainId = GenerateGrainId();

        var result = await EventStorage.AppendEventsToStorage(grainId, new[] { new SampleEvent() }, 10);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task Trying_to_read_from_a_version_past_the_last_version_returns_an_empty_list()
    {
        var grainId = GenerateGrainId();
        await EventStorage.AppendEventsToStorage(grainId, new[] { new SampleEvent() }, 0);

        var eventStream = EventStorage.ReadEventsFromStorage<SampleEvent>(grainId, 10);
        var eventList = await eventStream.ToListAsync();

        Assert.That(eventList, Is.Empty);
    }

    [Test]
    public async Task Reading_before_any_events_are_appended_returns_an_empty_list()
    {
        var grainId = GenerateGrainId();

        var eventStream = EventStorage.ReadEventsFromStorage<SampleEvent>(grainId);
        var eventList = await eventStream.ToListAsync();

        Assert.That(eventList, Is.Empty);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await Cluster.StopAllSilosAsync();
    }

    private GrainId GenerateGrainId() => GrainId.Create(nameof(SampleGrain), Guid.NewGuid().ToString());

    private class SampleGrain;

    private record SampleEvent(int Value = 0);

    private class TestSiloConfigurator : ISiloConfigurator
    {
        public void Configure(ISiloBuilder siloBuilder)
        {
            siloBuilder.AddMemoryEventStorage("MemoryEventStorage");
            siloBuilder.AddMemoryEventStorageAsDefault();
        }
    }
}