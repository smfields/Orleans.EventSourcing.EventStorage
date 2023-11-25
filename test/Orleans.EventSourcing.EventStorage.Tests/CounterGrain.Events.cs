namespace Orleans.EventSourcing.EventStorage.Tests.TestGrains;

public interface ICounterEvent;

[Serializable, GenerateSerializer]
public record CounterIncrementedEvent(uint Amount) : ICounterEvent;

[Serializable, GenerateSerializer]
public record CounterDecrementedEvent(uint Amount) : ICounterEvent;

[Serializable, GenerateSerializer]
public class CounterResetEvent : ICounterEvent;