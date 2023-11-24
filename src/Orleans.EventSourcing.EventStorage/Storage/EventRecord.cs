namespace Orleans.EventSourcing.EventStorage;

/// <summary>
/// Record of a single event
/// </summary>
/// <param name="EventData">Event data</param>
/// <param name="Version">Version number for the event</param>
/// <typeparam name="TEvent">Event type</typeparam>
public record EventRecord<TEvent>(TEvent EventData, int Version);