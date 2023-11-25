using Orleans.EventSourcing.EventStorage.Tests.TestGrains;

namespace Orleans.EventSourcing.EventStorage.Tests;

public class CounterGrainState
{
    public int Value { get; private set; } = 0;

    public void Apply(CounterResetEvent resetEvent)
    {
        Value = 0;
    }

    public void Apply(CounterIncrementedEvent incrementEvent)
    {
        Value = (int)(Value + incrementEvent.Amount);
    }

    public void Apply(CounterDecrementedEvent decrementEvent)
    {
        Value = (int)(Value - decrementEvent.Amount);
    }
}

public interface ICounterGrain : IGrainWithGuidKey
{
    public ValueTask Reset();
    public ValueTask Increment(uint amount);
    public ValueTask Decrement(uint amount);
    public ValueTask ConfirmEvents();
}

public class CounterGrain : JournaledGrain<CounterGrainState, ICounterEvent>, ICounterGrain
{
    public ValueTask Reset()
    {
        RaiseEvent(new CounterResetEvent());
        return ValueTask.CompletedTask;
    }

    public ValueTask Increment(uint amount)
    {
        if (WillOverflow(amount))
        {
            throw new OverflowException("Incrementing by the specified amount would cause an overflow");
        }

        RaiseEvent(new CounterIncrementedEvent(amount));
        return ValueTask.CompletedTask;
    }

    public ValueTask Decrement(uint amount)
    {
        if (WillUnderflow(amount))
        {
            throw new OverflowException("Decrementing by the specified amount would cause an underflow");
        }

        RaiseEvent(new CounterDecrementedEvent(amount));
        return ValueTask.CompletedTask;
    }

    async ValueTask ICounterGrain.ConfirmEvents()
    {
        await ConfirmEvents();
    }

    private bool WillOverflow(uint amount)
    {
        return int.MaxValue - State.Value >= amount;
    }

    private bool WillUnderflow(uint amount)
    {
        return int.MinValue + amount < State.Value;
    }
}