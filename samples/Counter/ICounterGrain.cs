namespace Counter;

public interface ICounterGrain : IGrainWithIntegerKey
{
    public ValueTask<int> GetCurrentValue();
    public ValueTask Increment(int amount);
    public ValueTask Decrement(int amount);
}