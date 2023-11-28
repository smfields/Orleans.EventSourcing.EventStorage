namespace Orleans.EventSourcing.EventStorage.Testing.TestGrains.CounterGrain;

public class CounterGrainState
{
    public int Value { get; private set; }

    public void Apply(CounterResetEvent resetEvent)
    {
        Value = resetEvent.ResetValue;
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
    public ValueTask Reset(int newValue = 0);
    public ValueTask<int> GetCurrentValue();
    public ValueTask Increment(uint amount);
    public ValueTask Decrement(uint amount);
    public ValueTask ConfirmEvents();
}

public class CounterGrain : JournaledGrain<CounterGrainState, ICounterEvent>, ICounterGrain
{
    public ValueTask Reset(int newValue = 0)
    {
        RaiseEvent(new CounterResetEvent(newValue));
        return ValueTask.CompletedTask;
    }

    public ValueTask<int> GetCurrentValue()
    {
        return ValueTask.FromResult(State.Value);
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
        try
        {
            checked
            {
                _ = State.Value + (int)amount;
                return false;
            }
        }
        catch (Exception)
        {
            return true;
        }
    }

    private bool WillUnderflow(uint amount)
    {
        try
        {
            checked
            {
                _ = State.Value - (int)amount;
                return false;
            }
        }
        catch (Exception)
        {
            return true;
        }
    }
}