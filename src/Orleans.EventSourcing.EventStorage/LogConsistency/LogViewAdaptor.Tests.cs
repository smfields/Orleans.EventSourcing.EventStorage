using Orleans.EventSourcing.EventStorage.Testing.TestGrains;

namespace Orleans.EventSourcing.EventStorage;

public class LogViewAdaptorTests
{
    private AutoMocker MockContainer { get; set; } = null!;

    [SetUp]
    public void Setup()
    {
        MockContainer = InitializeMockContainer();
    }

    private AutoMocker InitializeMockContainer(AutoMocker? mockContainer = null)
    {
        mockContainer ??= new AutoMocker();

        return mockContainer;
    }

    [Test]
    public void Initial_confirmed_version_is_0()
    {
        var logViewAdaptor = CreateLogViewAdaptor<CounterGrainState, ICounterEvent>();

        Assert.That(logViewAdaptor.ConfirmedVersion, Is.EqualTo(0));
    }

    // [Test]
    // public async Task OnActivate_initializes_the_confirmed_view()
    // {
    //     var initialEvents = new List<EventRecord<ICounterEvent>>
    //     {
    //         new(new CounterResetEvent(10), 1)
    //     };
    //     MockContainer
    //         .GetMock<IEventStorage>()
    //         .Setup(
    //             x => x.ReadEventsFromStorage<ICounterEvent>(
    //                 It.IsAny<GrainId>(),
    //                 It.IsAny<int>(),
    //                 It.IsAny<int>()
    //             )
    //         )
    //         .Returns(() => { });
    //     var logViewAdaptor = CreateLogViewAdaptor<CounterGrainState, ICounterEvent>();
    //
    //     await logViewAdaptor.PreOnActivate();
    //     await logViewAdaptor.PostOnActivate();
    //
    //     throw new NotImplementedException();
    //     // Assert.That(logViewAdaptor.ConfirmedView);
    // }

    private LogViewAdaptor<TLogView, TLogEntry> CreateLogViewAdaptor<TLogView, TLogEntry>()
        where TLogView : class, new()
        where TLogEntry : class
    {
        return MockContainer.CreateInstance<LogViewAdaptor<TLogView, TLogEntry>>();
    }
}