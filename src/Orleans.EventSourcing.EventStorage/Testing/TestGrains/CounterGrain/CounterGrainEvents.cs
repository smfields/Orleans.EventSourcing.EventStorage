namespace Orleans.EventSourcing.EventStorage.Testing.TestGrains.CounterGrain;

public interface ICounterEvent;

[Serializable, GenerateSerializer]
public record CounterIncrementedEvent(uint Amount) : ICounterEvent;

[Serializable, GenerateSerializer]
public record CounterDecrementedEvent(uint Amount) : ICounterEvent;

[Serializable, GenerateSerializer]
public record CounterResetEvent(int ResetValue) : ICounterEvent;