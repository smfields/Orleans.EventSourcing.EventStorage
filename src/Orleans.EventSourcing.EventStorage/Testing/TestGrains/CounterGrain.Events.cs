namespace Orleans.EventSourcing.EventStorage.Testing.TestGrains;

public interface ICounterEvent;

public record CounterIncrementedEvent(uint Amount) : ICounterEvent;

public record CounterDecrementedEvent(uint Amount) : ICounterEvent;

public record CounterResetEvent(int ResetValue) : ICounterEvent;