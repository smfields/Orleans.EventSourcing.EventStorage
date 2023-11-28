namespace Orleans.EventSourcing.EventStorage;

/// <summary>
/// Record of a single event
/// </summary>
/// <param name="Data">Event data</param>
/// <param name="Version">Version number for the event</param>
/// <typeparam name="TEvent">Event type</typeparam>
[GenerateSerializer, Serializable]
public record EventRecord<TEvent>(TEvent Data, int Version);