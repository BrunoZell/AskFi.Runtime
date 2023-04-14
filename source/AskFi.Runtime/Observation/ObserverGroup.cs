using System.Threading.Channels;
using AskFi.Persistence;
using AskFi.Runtime.Observation.Objects;
using AskFi.Runtime.Persistence;
using static AskFi.Runtime.DataModel;

namespace AskFi.Runtime.Observation;

/// <summary>
/// A single instance to pipe observations from all <see cref="ObserverInstance"/> (<see cref="Sdk.IObserver{Perception}"/>)
/// through in an async way. This is to sequence it in a first-come-first-server way. After new observations
/// are written to the <see cref="ObserverGroup"/>, their observation order is set, and all conclusions derived are
/// deterministic (and thus reproducible) thereafter.
/// </summary>
internal sealed class ObserverGroup
{
    private readonly IdeaStore _ideaStore;

    public ObserverGroup(IdeaStore ideaStore)
    {
        _ideaStore = ideaStore;
    }

    /// <summary>
    /// All <see cref="ObserverInstance"/> of this session (one per <see cref="Sdk.IObserver{Perception}"/>) write
    /// new observations into this queue, which is then sequentially consumes via <see cref="Sequence"/>.
    /// </summary>
    private readonly Channel<NewCapturedObservation> _incomingObservations = Channel.CreateUnbounded<NewCapturedObservation>();

    /// <summary>
    /// Accepts new observations from all <see cref="ObserverInstance"/> of this session.
    /// </summary>
    public ChannelWriter<NewCapturedObservation> ObservationSink => _incomingObservations.Writer;

    /// <summary>
    /// Long-running background task that reads all pooled new observations and builds <see cref="LinkedObservation"/> for each one.
    /// This introduces a relative ordering in time between observations of the same ObserverGroup.
    /// </summary>
    public async IAsyncEnumerable<ContentId> Sequence()
    {
        var latestObservations = new Dictionary<object, ContentId>();

        // Sequentially receives all observations from IObserver-instances in this group as they happen.
        await foreach (var newObservation in _incomingObservations.Reader.ReadAllAsync()) {
            latestObservations[newObservation.ObserverInstance] = newObservation.CapturedObservationCid;

            var relativeTimeLinks = latestObservations
                .Where(kvp => !ReferenceEquals(kvp.Key, newObservation.ObserverInstance))
                .Select(o => new RelativeTimeLink(o.Value))
                .ToArray();

            var linkedObservation = new LinkedObservation(newObservation.CapturedObservationCid, relativeTimeLinks);

            // Persist and implicitly publish to downstream system (to later query by hash if desired)
            var linkedObservationCid = await _ideaStore.Store(linkedObservation);

            yield return linkedObservationCid;
        }
    }
}
