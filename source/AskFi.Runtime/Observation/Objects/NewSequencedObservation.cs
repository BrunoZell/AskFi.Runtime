using AskFi.Persistence;

namespace AskFi.Runtime.Observation.Objects;

/// <summary>
/// Event payload for event raised on new observations from an IObserver-instance.
/// This event is fired after Observer-scoped sequencing, so it carries the reference
/// to the latest <see cref="DataModel.ObservationSequenceHead{Perception}"/> with the
/// new <see cref="Sdk.IObserver{Perception}"/> as latest node.
/// </summary>
internal class NewSequencedObservation
{
    /// <summary>
    /// Absolute timestamp of when this observation was recorded.
    /// As of runtime clock.
    /// </summary>
    public required DateTime ObservationTimestamp { get; init; }

    /// <summary>
    /// The P from IObserver<P> (type of the originating observer instance)
    /// </summary>
    public required Type PerceptionType { get; init; }

    /// <summary>
    /// Cid to the latest ObservationSequenceHead<this.PerceptionType> with new observation as first node
    /// </summary>
    public required ContentId ObservationSequenceHeadCid { get; init; }
}
