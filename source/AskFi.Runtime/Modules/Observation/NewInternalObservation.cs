using AskFi.Runtime.Persistence;

namespace AskFi.Runtime.Modules.Observation;

/// <summary>
/// Event payload for event raised on new observations from an IObserver-instance.
/// This message is produced in <see cref="Observation.ObserverInstance"/> and
/// received in <see cref="ObserverModule"/>. It is only used internally within the
/// observer module.
/// </summary>
internal sealed class NewInternalObservation
{
    /// <summary>
    /// Obserer instance
    /// </summary>
    public required object ObserverInstance { get; init; } // IObserver<P> instance

    /// <summary>
    /// Cid to the newly produced CapturedObservation'Percept>
    /// </summary>
    public required ContentId CapturedObservationCid { get; init; }

    /// <summary>
    /// Type of 'Percept
    /// </summary>
    public required Type PerceptType { get; init; }
}
