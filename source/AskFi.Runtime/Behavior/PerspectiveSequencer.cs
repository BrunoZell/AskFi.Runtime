using System.Threading.Channels;
using AskFi.Runtime.Objects;
using AskFi.Runtime.Queries;
using static AskFi.Runtime.DataModel;
using static AskFi.Sdk;

namespace AskFi.Runtime.Behavior;

/// <summary>
/// A single instance to pipe observations from all <see cref="ObserverSequencer"/> (<see cref="Sdk.IObserver{Perception}"/>)
/// through in an async way. This is to sequence it in a first-come-first-server way. After new observations
/// are written to the <see cref="PerspectiveSequencer"/>, their observation order is set, and all conclusions derived are
/// deterministic (and thus reproducible) thereafter.
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
    /// then wraps it into a <see cref="Perspective"/> and returns it.
    /// </summary>
    public async IAsyncEnumerable<Perspective> Sequence()
    {
        var perspectiveSequence = PerspectiveSequenceHead.Empty;
        var perspectiveSequenceHash = PreviousPerspectiveHash.None;

        await foreach (var newObservation in _incomingObservations.Reader.ReadAllAsync()) {
            var timestamp = DateTime.UtcNow;
            var newObservationSequenceHead = newObservation.ObservationSequenceHead;

            // Append happening to this worlds event sequence
            perspectiveSequence = PerspectiveSequenceHead.NewHappening(timestamp, _previous: perspectiveSequenceHash, newObservationSequenceHead);

            // Persist and implicitly publish to downstream query system (to later query by hash if desired)
            perspectiveSequenceHash = PerspectiveSequenceStore.Store(perspectiveSequence);

            var perspective = new Perspective(perspectiveSequenceHash.raw, new PerspectiveQueries(perspectiveSequenceHash.raw));
            yield return perspective;
        }
    }
}
