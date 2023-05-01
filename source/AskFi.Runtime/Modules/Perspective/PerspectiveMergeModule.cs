using System.Threading.Channels;
using AskFi.Runtime.Messages;
using AskFi.Runtime.Persistence;
using AskFi.Runtime.Platform;
using Microsoft.FSharp.Collections;
using static AskFi.Runtime.DataModel;

namespace AskFi.Runtime.Modules.Perspective;
internal class PerspectiveMergeModule
{
    private readonly ChannelReader<NewPerspective> _input;
    private readonly Channel<NewPerspective> _output;
    private readonly IPlatformPersistence _persistence;

    public ChannelReader<NewPerspective> Output => _output.Reader;

    public PerspectiveMergeModule(
        IPlatformPersistence persistence,
        ChannelReader<NewPerspective> input)
    {
        _input = input;
        _output = Channel.CreateUnbounded<NewPerspective>();
        _persistence = persistence;
    }

    public async Task Run(CancellationToken cancellationToken)
    {
        var emptyPerspective = PerspectiveSequenceHead.Beginning;
        var emptyPerspectiveCid = _persistence.Cid(emptyPerspective);
        var observationPool = new ObservationPool(
            aggregatePerspective: emptyPerspectiveCid,
            droppedPerspectives: new FSharpSet<ContentId>(Enumerable.Empty<ContentId>()));

        await foreach (var newPerspective in _input.ReadAllAsync(cancellationToken)) {
            // Merge perspectives via the aggregatable observation pool CRDT
            var incomingObservationPool = await _persistence.Get<ObservationPool>(newPerspective.ObservationPool);
            var mergedObservationPool = ObservationPoolMerger.Add(observationPool, incomingObservationPool, _persistence);
            var mergedObservationPoolCid = _persistence.Cid(observationPool);

            var newMergedPerspective = new NewPerspective(mergedObservationPoolCid);

            await _output.Writer.WriteAsync(newMergedPerspective);
        }
    }
}
