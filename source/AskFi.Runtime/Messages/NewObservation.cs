using AskFi.Persistence;

namespace AskFi.Runtime.Messages;

/// <summary>
/// Message from Observer Module emitted on each new received observation.
/// </summary>
internal sealed class NewObservation
{
    /// <summary>
    /// The 'Percept from IObserver<'Percept> to full parse the <see cref="DataModel.LinkedObservation"/>.
    /// </summary>
    public required Type PerceptionType { get; init; }

    /// <summary>
    /// Cid to the newly produced LinkedObservation
    /// </summary>
    public required ContentId LinkedObservationCid { get; init; }
}
