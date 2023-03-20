using AskFi.Sdk;

namespace AskFi.Runtime.Internal;

/// <summary>
/// Runtime-version of <see cref="ObservationSession"/>, where the uniqueness-requirement
/// of ObservationSessions underlying binary data is scoped to a single IObserver-instance.
/// Thus, to address all ObservationSessions of a Askbot Instance, the key is [observer-instance][session-key].
/// </summary>
internal class ObservationSessionKey
{
    public object ObserverInstance { get; init; }
    public ObservationSession ObserverProvidedSessionKey { get; init; }
}
