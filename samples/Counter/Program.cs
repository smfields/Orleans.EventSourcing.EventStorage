using Counter;

// Configure the host
using var host = new HostBuilder()
    .UseOrleans(builder =>
    {
        builder
            .UseLocalhostClustering()
            .AddEventStorageBasedLogConsistencyProviderAsDefault()
            .AddMemoryEventStorageAsDefault();
    })
    .Build();

// Start the host
await host.StartAsync();

// Get the grain factory
var grainFactory = host.Services.GetRequiredService<IGrainFactory>();

// Get a reference to the counter grain
var counter = grainFactory.GetGrain<ICounterGrain>(0);

// Print the initial value
var initialValue = await counter.GetCurrentValue();
Console.WriteLine($"Initial Value: {initialValue}");

// Increment the grain
const int incrementAmount = 15;
Console.WriteLine($"Incrementing by {incrementAmount}");
await counter.Increment(15);

// Retrieve the updated value
var updatedValue = await counter.GetCurrentValue();
Console.WriteLine($"Updated Value: {updatedValue}");

Console.WriteLine("Orleans is running.");
Console.WriteLine("Press Enter to terminate...");

await host.StopAsync();