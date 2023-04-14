using System.Threading.Channels;
using AskFi.Runtime.Observation.Objects;
using AskFi.Runtime.Persistence;
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
    private readonly IdeaStore _ideaStore;
    private readonly StateTrace _stateTrace;

    public PerspectiveSequencer(IdeaStore ideaStore, StateTrace stateTrace)
    {
        _ideaStore = ideaStore;
        _stateTrace = stateTrace;
    }

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
        var perspectiveSequenceCid = await _ideaStore.Store(perspectiveSequence);

        await foreach (var newObservation in _incomingObservations.Reader.ReadAllAsync()) {
            // Append the updated Observation Sequence as a new happening to the Perspective, as a new sequence head.
            perspectiveSequence = PerspectiveSequenceHead.NewHappening(new PerspectiveSequenceNode(
                at: newObservation.ObservationTimestamp,
                previous: perspectiveSequenceCid,
                observationSequenceHead: newObservation.ObservationSequenceHeadCid,
                observationPerceptionType: newObservation.PerceptionType));

            // Persist and implicitly publish to downstream query system (to later query by hash if desired)
            perspectiveSequenceCid = await _ideaStore.Store(perspectiveSequence);

            yield return new Perspective(perspectiveSequenceCid, new ObservationQueries(perspectiveSequenceCid, _ideaStore));

            _stateTrace.LatestPerspectiveSequence = perspectiveSequenceCid;
        }
    }
}
