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
    /// The P from IObserver<P> (type of the originating observer instance)
    /// </summary>
    public required Type PerceptionType { get; init; }

    /// <summary>
    /// Of type ObservationSequenceHead<this.PerceptionType> with this.Observation as first node
    /// </summary>
    public required object ObservationSequenceHead { get; init; }

    /// <summary>
    /// Of type Sdk.Observation<this.PerceptionType>
    /// </summary>
    public required object Observation { get; init; }
}
