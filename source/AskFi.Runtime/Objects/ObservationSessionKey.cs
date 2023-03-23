using static AskFi.Sdk;

namespace AskFi.Runtime.Objects;

/// <summary>
/// Runtime-version of <see cref="ObservationSession"/>, where the uniqueness-requirement
/// of ObservationSessions underlying binary data is scoped to a single IObserver-instance.
/// Thus, to address all ObservationSessions of a Askbot Instance, the key is [observer-instance][session-key].
/// </summary>
internal class ObservationSessionKey
{
    /// <summary>
    /// In-process identity of the IObserver<P>-instance this session originated from.
    /// </summary>
    public required object ObserverInstance { get; init; }

    /// <summary>
    /// Correlation id of an observation session. Scoped to the IObserver<P>-instance <see cref="ObserverInstance"/>.
    /// </summary>
    public required ObservationSession ObserverProvidedSessionKey { get; init; }
}
