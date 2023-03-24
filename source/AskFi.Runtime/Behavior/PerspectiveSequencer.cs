using System.Threading.Channels;
using AskFi.Runtime.Objects;
using AskFi.Runtime.Queries;
using static AskFi.Runtime.DataModel;
using static AskFi.Sdk;

namespace AskFi.Runtime.Behavior;

/// <summary>
/// A single instance to pipe observations from all <see cref="ObserverSequencer"/> (<see cref="Sdk.IObserver{Perception}"/>)
/// through in an asycnt way. This is to sequence it in a first-come-first-server way. After new observations
/// are written to the <see cref="PerspectiveSequencer"/>, their observation order is set, and all conclusions derived are
/// deterministic (and thus reproducable) thereafter.
/// </summary>
internal class PerspectiveSequencer
{
    /// <summary>
    /// All <see cref="ObserverSequencer"/> of this session (one per <see cref="Sdk.IObserver{Perception}"/>) write
    /// new observations into this queue, which is then sequentially consumes via <see cref="Sequence"/>.
    /// </summary>
    private readonly Channel<NewSequencedObservation> _incomingObservations = Channel.CreateUnbounded<NewSequencedObservation>();

    /// <summary>
    /// Accepts new observations from all <see cref="ObserverSequencer"/> of this session.
    /// </summary>
    public ChannelWriter<NewSequencedObservation> ObservationSink => _incomingObservations.Writer;

    /// <summary>
    /// Long-running background task that reads all pooled new observations and builds the <see cref="PerspectiveSequenceHead"/> of this session,
    /// then wraps it into a <see cref="WorldState"/> and returns it.
    /// </summary>
    public async IAsyncEnumerable<WorldState> Sequence()
    {
        var eventSequence = PerspectiveSequenceHead.Empty;
        var eventSequenceHash = 0;

        await foreach (var newObservation in _incomingObservations.Reader.ReadAllAsync()) {
            var timestamp = DateTime.UtcNow;
            var latestObservationSequenceHead = newObservation.ObservationSequenceHead;

            // Append happening to this worlds event sequence
            eventSequence = PerspectiveSequenceHead.NewHappening(timestamp, _previous: eventSequenceHash, latestObservationSequenceHead);

            // Persist and implicitly publish to downstream query system (to later query by hash if desired)
            eventSequenceHash = WorldEventStore.Store(eventSequence);

            var latestEventSequenceHash = eventSequence.GetHashCode();
            var state = new WorldState(latestEventSequenceHash);
            yield return state;
        }
    }
}
