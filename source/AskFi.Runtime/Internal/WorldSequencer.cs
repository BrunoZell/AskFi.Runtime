using System.Threading.Channels;
using static AskFi.Runtime.DataModel;
using static AskFi.Sdk;

namespace AskFi.Runtime.Internal;

/// <summary>
/// A single instance to pipe observations from all <see cref="ObserverSequencer"/> (<see cref="Sdk.IObserver{Perception}"/>)
/// through in an asycnt way. This is to sequence it in a first-come-first-server way. After new observations
/// are written to the <see cref="WorldSequencer"/>, their observation order is set, and all conclusions derived are
/// deterministic (and thus reproducable) thereafter.
/// </summary>
internal class WorldSequencer
{
    /// <summary>
    /// All <see cref="ObserverSequencer"/> of this session (one per <see cref="Sdk.IObserver{Perception}"/>) write
    /// new observations into this queue, which is then sequentially consumes via <see cref="Sequence"/>.
    /// </summary>
    private readonly Channel<OnNewObservation> _incomingObservations = Channel.CreateUnbounded<OnNewObservation>();

    /// <summary>
    /// Accepts new observations from all <see cref="ObserverSequencer"/> of this session.
    /// </summary>
    public ChannelWriter<OnNewObservation> ObservationSink => _incomingObservations.Writer;

    /// <summary>
    /// Long-running background task that reads all pooled new observations and builds the <see cref="WorldEventSequence"/> of this session,
    /// then wraps it into a <see cref="WorldState"/> and returns it.
    /// </summary>
    public async IAsyncEnumerable<WorldState> Sequence()
    {
        var eventSequence = WorldEventSequence.Empty;

        await foreach (var newObservation in _incomingObservations.Reader.ReadAllAsync()) {
            var timestamp = (ulong)DateTime.UtcNow.Ticks;
            var previousHash = eventSequence.GetHashCode();
            var latestObservationStreamHead = newObservation.ObservationStreamHead;

            // Append happening to this worlds event sequence
            eventSequence = WorldEventSequence.NewHappening(timestamp, previousHash, _nonce: 0ul, latestObservationStreamHead);

            // Todo: Send world event tree to persistence subsystem

            var latestEventSequenceHash = eventSequence.GetHashCode();
            var state = new WorldState(latestEventSequenceHash);
            yield return state;
        }
    }
}
