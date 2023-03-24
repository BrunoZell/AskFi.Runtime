namespace AskFi.Runtime.Objects;

/// <summary>
/// Event payload for event raised on new observations from an IObserver-instance.
/// This event is fired after Observer-scoped sequencing, so it carries the reference
/// to that <see cref="Sdk.IObserver{Perception}"/>'s latest <see cref="DataModel.ObservationSequenceHead{Perception}"/>.
/// </summary>
internal class OnNewObservation
{
    public required Type PerceptionType { get; init; } // the P from IObserver<P> (type of the originating observer instance)
    public required ObservationSessionKey Session { get; init; }
    public required object ObservationSequenceHead { get; init; } // of type ObservationSequenceHead<this.PerceptionType>
}
