using Counter;
using Orleans.EventSourcing.EventStorage.Memory;

// Configure the host
using var host = new HostBuilder()
    .UseOrleans(builder =>
    {
        builder.UseLocalhostClustering();

        // Log Storage Provider
        // builder.AddLogStorageBasedLogConsistencyProviderAsDefault();
        // builder.AddMemoryGrainStorageAsDefault();

        // Event Storage Provider
        builder.AddEventStorageBasedLogConsistencyProviderAsDefault();
        builder.AddMemoryEventStorageAsDefault();
    })
    .Build();

// Start the host
await host.StartAsync();

// Get the grain factory
var grainFactory = host.Services.GetRequiredService<IGrainFactory>();

var memory = grainFactory.GetGrain<IMemoryEventStorageGrain>(0);

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

Console.WriteLine("Orleans is running.\nPress Enter to terminate...");
Console.ReadLine();
Console.WriteLine("Orleans is stopping...");

await host.StopAsync();