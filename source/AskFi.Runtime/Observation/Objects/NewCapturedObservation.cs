using AskFi.Persistence;

namespace AskFi.Runtime.Observation.Objects;

/// <summary>
/// Event payload for event raised on new observations from an IObserver-instance.
/// This event is fired after Observer-scoped sequencing, so it carries the reference
/// to the latest <see cref="DataModel.ObservationSequenceHead{Perception}"/> with the
/// new <see cref="Sdk.IObserver{Perception}"/> as latest node.
/// </summary>
internal sealed class NewCapturedObservation
{
    /// <summary>
    /// Obserer instance
    /// </summary>
    public required object ObserverInstance { get; init; } // IObserver<P> instance

    /// <summary>
    /// Cid to the newly produced CapturedObservation'Perception>
    /// </summary>
    public required ContentId CapturedObservationCid { get; init; }
}
