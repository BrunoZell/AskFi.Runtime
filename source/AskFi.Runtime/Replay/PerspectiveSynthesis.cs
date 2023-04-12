using AskFi.Persistence;
using AskFi.Runtime.Persistence;
using static AskFi.Runtime.DataModel;

namespace AskFi.Runtime.Replay;
public class PerspectiveSynthesis
{
    public PerspectiveSynthesis((Type, ContentId)[] observationSequences)
    {

    }

    private class ObservationSequenceReader<TPerception>
    {
        private readonly ContentId _latestObservationSequenceCid;
        private readonly IdeaStore _ideaStore;

        public ObservationSequenceReader(ContentId latestObservationSequenceCid, IdeaStore ideaStore)
        {
            _latestObservationSequenceCid = latestObservationSequenceCid;
            _ideaStore = ideaStore;
        }

        public async IAsyncEnumerable<ObservationSequenceHead<TPerception>> Read()
        {
            var headCid = _latestObservationSequenceCid;

            while (true) {
                var head = await _ideaStore.Load<ObservationSequenceHead<TPerception>>(_latestObservationSequenceCid);

                yield return head;

                if (head.IsBeginning) {
                    yield break;
                }

                if (head is ObservationSequenceHead<TPerception>.Observation observation) {
                    headCid = observation.Item.Previous;
                } else {
                    yield break;
                }
            }
        }
    }
}
