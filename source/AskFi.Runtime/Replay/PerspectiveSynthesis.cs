using AskFi.Persistence;
using AskFi.Runtime.Observation.Objects;
using AskFi.Runtime.Persistence;
using AskFi.Runtime.Queries;
using static AskFi.Runtime.DataModel;
using static AskFi.Sdk;

namespace AskFi.Runtime.Replay;

internal class PerspectiveSynthesis
{
    private readonly Type _perception;
    private readonly ContentId _initialObservationSequenceCid;
    private readonly IdeaStore _ideaStore;

    // Todo: Allow for merging of multiple Observation Sequences accross different 'Perception.

    public PerspectiveSynthesis(Type perception, ContentId initialObservationSequenceCid, IdeaStore ideaStore)
    {
        _perception = perception;
        _initialObservationSequenceCid = initialObservationSequenceCid;
        _ideaStore = ideaStore;
    }

    public async IAsyncEnumerable<Perspective> Sequence()
    {
        var allObservations = await CreateObservationSequenceReader(_perception, _initialObservationSequenceCid, _ideaStore).ToListAsync();
        allObservations.Reverse(); // The reader above reads backwards in time. Perspective Sequences must be built forward in time.

        var perspectiveSequence = PerspectiveSequenceHead.Empty;
        var perspectiveSequenceCid = await _ideaStore.Store(perspectiveSequence);

        foreach (var newObservation in allObservations) {
            var timestamp = null as DateTime; // Todo: Read timestamp from Observation

            // Append the updated Observation Sequence as a new happening to the Perspective, as a new sequence head.
            perspectiveSequence = PerspectiveSequenceHead.NewHappening(new PerspectiveSequenceNode(
                at: timestamp,
                previous: perspectiveSequenceCid,
                observationSequenceHead: newObservation.ObservationSequenceHeadCid,
                observationPerceptionType: newObservation.PerceptionType));

            // Persist and implicitly publish to downstream query system (to later query by hash if desired)
            perspectiveSequenceCid = await _ideaStore.Store(perspectiveSequence);

            yield return new Perspective(perspectiveSequenceCid, new PerspectiveQueries(perspectiveSequenceCid, _ideaStore));
        }
    }

    private static IAsyncEnumerable<NewSequencedObservation> CreateObservationSequenceReader(Type perception, ContentId initialCid, IdeaStore ideaStore)
    {
        var readerType = typeof(ObservationSequenceReader<>).MakeGenericType(perception);
        var reader = Activator.CreateInstance(readerType, initialCid, ideaStore);
        return Read((dynamic)reader);
    }

    private static IAsyncEnumerable<NewSequencedObservation> Read<TPerception>(ObservationSequenceReader<TPerception> reader) =>
        reader.ReverseSequence();

    private class ObservationSequenceReader<TPerception>
    {
        private readonly ContentId _initialObservationSequenceHeadCid;
        private readonly IdeaStore _ideaStore;

        public ObservationSequenceReader(ContentId initialObservationSequenceCid, IdeaStore ideaStore)
        {
            _initialObservationSequenceHeadCid = initialObservationSequenceCid;
            _ideaStore = ideaStore;
        }

        public async IAsyncEnumerable<NewSequencedObservation> ReverseSequence()
        {
            var observationSequenceHeadCid = _initialObservationSequenceHeadCid;

            while (true) {
                var head = await _ideaStore.Load<ObservationSequenceHead<TPerception>>(_initialObservationSequenceHeadCid);

                yield return new NewSequencedObservation() {
                    PerceptionType = typeof(TPerception),
                    ObservationSequenceHeadCid = observationSequenceHeadCid
                };

                if (head.IsBeginning) {
                    yield break;
                }

                if (head is ObservationSequenceHead<TPerception>.Observation observation) {
                    observationSequenceHeadCid = observation.Node.Previous;
                } else {
                    yield break;
                }
            }
        }
    }
}
