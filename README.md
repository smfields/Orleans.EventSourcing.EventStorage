# Orleans.EventSourcing.EventStorage

## Getting Started

Install from NuGet:

`Install-Package Orleans.EventSourcing.EventStorage`

or

`dotnet add package Orleans.EventSourcing.EventStorage`

Configure your silo:

```csharp
builder.Host.UseOrleans(siloBuilder =>
{
    siloBuilder.UseLocalhostClustering();
    siloBuilder.AddEventStorageBasedLogConsistencyProviderAsDefault();
    siloBuilder.AddMemoryEventStorageAsDefault();
});
```

## FAQ

### What is Orleans?

Orleans is a cross-platform framework for building robust, scalable distributed applications.

[Documentation](https://github.com/dotnet/orleans#orleans-is-a-cross-platform-framework-for-building-robust-scalable-distributed-applications)

### What is Event Sourcing?

Event sourcing is approach to handling operations on data that's driven by a sequence of events, each of which is
recorded in an append-only store.

Read more:

- [Azure Architecture Center - Event Sourcing Pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/event-sourcing)
- [Martin Fowler - Event Sourcing](https://martinfowler.com/eaaDev/EventSourcing.html)
- [Microservices.io - Pattern: Event sourcing](https://microservices.io/patterns/data/event-sourcing.html)

### Why use Event Sourcing with Orleans?

Not only does the Orleans framework make it extremely easy to implement the event sourcing pattern, but they also
compliment each other really well. Orleans can solve many of the issues that come along with using event sourcing, and
vice versa.

Orleans is not great at doing queries over grain data. Event sourcing can help resolve this shortcoming by using it
alongside [CQRS](https://microservices.io/patterns/data/cqrs.html). The combination of event sourcing and CQRS means
that you can implement a write side (commands) using orleans grains, while also having a read side (queries) that are
simply a projection of the events in the event store.

Orleans also struggles with certain aspects of event driven architectures, such as reliably publishing events if and
only if a write is successful. In many traditional architectures this is achieved using
a [transactional outbox](https://microservices.io/patterns/data/transactional-outbox.html), which is
unfortunately quite complex to implement in Orleans due to the way grain data is persisted by default. Using event
sourcing with your Orleans grains resolves this issue since the state being stored and the event to be published are one
and the same.

Orleans can also help with some of the shortcomings of traditional event sourcing. One of the common struggles with
event sourcing is the need to rehydrate the current state of an entity from the events stored in the event store. If the
stream of events is long, this can become a costly operation. Orleans helps in this regard because it can keep grains
alive and incrementally apply updates to the activated grains, reducing the number of times that the entire event stream
needs to be read and applied.

### Why not use the built-in log-consistency providers?

Orleans currently ships with 3 built-in log-consistency providers.

#### State storage

Stores grain snapshots, which is to say that it stores only the most recent version of the grain state in storage. In
many ways this is not true event sourcing because it does not actually persist the log of events.

#### Log storage

Stores the complete event sequence as a single object. While this does store all the events in the log, it is **not
recommended for production use** because it stores the entire sequence in a single entry that must be
read/written every time the entry is accessed.

#### Custom storage

Custom storage is flexible provider that allows the developer to plug-in their own storage mechanism while assuming very
little about how the events will be stored. While this can be an effective option, it has its downsides:

- It requires implementing
  the [ICustomStorageInterface<TState,TDelta>](https://learn.microsoft.com/en-us/dotnet/api/orleans.eventsourcing.customstorage.icustomstorageinterface-2?view=orleans-7.0)
  interface in your grains. This means your grains become responsible for managing their own storage, which is a
  violation of the single-responsibility principle and separation of concerns.
- A different implementation of ICustomStorageInterface<TState,TDelta> is required for every grain type that needs to be
  stored.
- It's a heavy-handed approach that isn't necessary for most people for whom a more pre-configured solution would
  suffice.
- The custom storage provider doesn't support `RetrieveConfirmedEvents`.