using NUnit.Framework.Internal.Execution;
using Orleans.EventSourcing.EventStorage.Testing.TestGrains;
using Orleans.Runtime;

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

    [Test]
    public async Task OnActivate_updates_the_confirmed_version()
    {
        var initialEvents = new List<EventRecord<ICounterEvent>>
        {
            new(new CounterResetEvent(1001), 1),
            new(new CounterIncrementedEvent(10), 2)
        };
        MockContainer
            .GetMock<IEventStorage>()
            .Setup(
                x => x.ReadEventsFromStorage<ICounterEvent>(
                    It.IsAny<GrainId>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()
                )
            )
            .Returns(() => initialEvents.ToAsyncEnumerable());
        var logViewAdaptor = CreateLogViewAdaptor<CounterGrainState, ICounterEvent>();

        await logViewAdaptor.PreOnActivate();
        await logViewAdaptor.PostOnActivate();
        await logViewAdaptor.Synchronize();

        Assert.That(logViewAdaptor.ConfirmedVersion, Is.EqualTo(2));
    }

    [Test]
    public async Task OnActivate_applies_stored_events_to_the_grain()
    {
        var initialEvents = new List<EventRecord<ICounterEvent>>
        {
            new(new CounterResetEvent(1001), 1),
            new(new CounterIncrementedEvent(10), 2)
        };
        MockContainer
            .GetMock<IEventStorage>()
            .Setup(
                x => x.ReadEventsFromStorage<ICounterEvent>(
                    It.IsAny<GrainId>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()
                )
            )
            .Returns(() => initialEvents.ToAsyncEnumerable());

        var logViewAdaptor = CreateLogViewAdaptor<CounterGrainState, ICounterEvent>();

        await logViewAdaptor.PreOnActivate();
        await logViewAdaptor.PostOnActivate();
        await logViewAdaptor.Synchronize();

        var hostGrainMock = MockContainer.GetMock<ILogViewAdaptorHost<CounterGrainState, ICounterEvent>>();
        foreach (var @event in initialEvents)
        {
            hostGrainMock.Verify(x => x.UpdateView(It.IsAny<CounterGrainState>(), @event.Data), Times.Once);
        }
    }

    private LogViewAdaptor<TLogView, TLogEntry> CreateLogViewAdaptor<TLogView, TLogEntry>()
        where TLogView : class, new()
        where TLogEntry : class
    {
        return MockContainer.CreateInstance<LogViewAdaptor<TLogView, TLogEntry>>();
    }
}