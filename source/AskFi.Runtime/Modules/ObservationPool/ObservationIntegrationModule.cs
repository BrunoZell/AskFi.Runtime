using System.Threading.Channels;
using AskFi.Runtime.Messages;
using AskFi.Runtime.Persistence;
using AskFi.Runtime.Platform;
using Microsoft.FSharp.Collections;
using static AskFi.Runtime.DataModel;

namespace AskFi.Runtime.Modules.Perspective;

/// <summary>
/// Turns NewObservation messages into NewObservationPool messages given that they indeed contained new information.
/// </summary>
internal class ObservationIntegrationModule
{
    private readonly ChannelReader<NewObservation> _input;
    private readonly Channel<NewObservationPool> _output;
    private readonly IPlatformPersistence _persistence;

    public ChannelReader<NewObservationPool> Output => _output.Reader;

    public ObservationIntegrationModule(
        IPlatformPersistence persistence,
        ChannelReader<NewObservation> input)
    {
        _output = Channel.CreateUnbounded<NewObservationPool>();
        _persistence = persistence;
        _input = input;
    }

    public async Task Run(CancellationToken cancellationToken)
    {
        // Local pool starts out with an empty pool
        var localHeaviestObservationPool = new KnowledgeBase(observations: null, actions: null);
        var localHeaviestObservationPoolCid = _persistence.Cid(localHeaviestObservationPool);

        await foreach (var observation in _input.ReadAllAsync(cancellationToken)) {
            // Transform incoming observation into observation pool to merge
            var incomingObservationPool = new KnowledgeBase(
                observations: new FSharpMap<ContentId, FSharpList<ContentId>>(
                    elements: new[] { new Tuple<ContentId, FSharpList<ContentId>>(
                        item1: observation.Identity,
                        item2: new FSharpList<ContentId>(observation.Head, FSharpList<ContentId>.Empty)) }),
                actions: null);

            // Merge incoming pool with local pool, creating a new heaviest local pool
            var mergedObservationPool = await KnowledgeBaseMerge.Join(localHeaviestObservationPool, incomingObservationPool, _persistence);
            var mergedObservationPoolCid = _persistence.Cid(mergedObservationPool);

            if (!mergedObservationPoolCid.Raw.Equals(localHeaviestObservationPoolCid.Raw)) {
                // Found new information.
                localHeaviestObservationPool = mergedObservationPool;
                localHeaviestObservationPoolCid = mergedObservationPoolCid;

                // Share it with others.
                var newObservationPool = new NewObservationPool(mergedObservationPoolCid);
                await _output.Writer.WriteAsync(newObservationPool);
            }
        }
    }
}
