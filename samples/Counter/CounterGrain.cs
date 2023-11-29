using Counter.Events;
using Orleans.EventSourcing;

namespace Counter;

public class CounterGrainState
{
    public int Value { get; private set; }

    public void Apply(CounterIncrementedEvent incrementedEvent)
    {
        Value += incrementedEvent.Amount;
    }

    public void Apply(CounterDecrementedEvent decrementedEvent)
    {
        Value -= decrementedEvent.Amount;
    }
}

public class CounterGrain : JournaledGrain<CounterGrainState>, ICounterGrain
{
    public ValueTask<int> GetCurrentValue()
    {
        return ValueTask.FromResult(State.Value);
    }

    public async ValueTask Increment(int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than 0");

        RaiseEvent(new CounterIncrementedEvent(amount));
        await ConfirmEvents();
    }

    public async ValueTask Decrement(int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than 0");

        RaiseEvent(new CounterDecrementedEvent(amount));
        await ConfirmEvents();
    }
}