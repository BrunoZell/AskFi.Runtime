using System.Threading.Channels;
using AskFi.Runtime.Messages;
using AskFi.Runtime.Platform;
using static AskFi.Runtime.DataModel;

namespace AskFi.Runtime.Modules.Perspective;

/// <summary>
/// Deduplicates NewObservationPool messages by only emitting those that added information to the locally ever growing pool.
/// </summary>
internal class ObservationDeduplicationModule
{
    private readonly ChannelReader<NewObservationPool> _input;
    private readonly Channel<NewObservationPool> _output;
    private readonly IPlatformPersistence _persistence;

    public ChannelReader<NewObservationPool> Output => _output.Reader;

    public ObservationDeduplicationModule(
        IPlatformPersistence persistence,
        ChannelReader<NewObservationPool> input)
    {
        _output = Channel.CreateUnbounded<NewObservationPool>();
        _persistence = persistence;
        _input = input;
    }

    public async Task Run(CancellationToken cancellationToken)
    {
        // Local pool starts out with an empty pool
        var localHeaviestObservationPool = new KnowledgeBase(
            observations: null,
            actions: null);

        var localHeaviestObservationPoolCid = _persistence.Cid(localHeaviestObservationPool);

        await foreach (var pool in _input.ReadAllAsync(cancellationToken)) {
            // Merge incoming pool with local pool, creating a new heaviest local pool
            var incomingKnowledgeBase = await _persistence.Get<KnowledgeBase>(pool.ObservationPool);
            var mergedKnowledgeBase = await KnowledgeBaseMerge.Join(localHeaviestObservationPool, incomingKnowledgeBase, _persistence);
            var mergedKnowledgeBaseCid = _persistence.Cid(mergedKnowledgeBase);

            if (!mergedKnowledgeBaseCid.Raw.Equals(localHeaviestObservationPoolCid.Raw)) {
                // Found new information.
                localHeaviestObservationPool = mergedKnowledgeBase;
                localHeaviestObservationPoolCid = mergedKnowledgeBaseCid;

                // Share it with others.
                var newObservationPool = new NewObservationPool(mergedKnowledgeBaseCid);
                await _output.Writer.WriteAsync(newObservationPool);
            }
        }
    }
}
