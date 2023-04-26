using System.Threading.Channels;
using AskFi.Runtime.Messages;
using AskFi.Runtime.Modules.Perspective;
using AskFi.Runtime.Platform;
using static AskFi.Runtime.DataModel;
using static AskFi.Sdk;

namespace AskFi.Runtime.Modules.Strategy;

internal class StrategyModule : IStrategyModule
{
    private readonly Func<Reflection, Sdk.Perspective, Decision> _strategy;
    private readonly Channel<NewPerspective> _input;
    private readonly Channel<NewDecision> _output;
    private readonly IPlatformPersistence _persistence;

    ChannelWriter<NewPerspective> IStrategyModule.Input { get; }
    ChannelReader<NewDecision> IStrategyModule.Output { get; }

    public StrategyModule(
        Func<Reflection, Sdk.Perspective, Decision> strategy,
        IPlatformPersistence persistence)
    {
        _strategy = strategy;
        _input = Channel.CreateUnbounded<NewPerspective>();
        _output = Channel.CreateUnbounded<NewDecision>();
        _persistence = persistence;
    }

    public async Task Run(CancellationToken sessionShutdown)
    {
        var decisionSequence = DecisionSequenceHead.Start;
        var decisionSequenceCid = await _persistence.Put(decisionSequence);

        await foreach (var newPerspective in _input.Reader.ReadAllAsync(sessionShutdown)) {
            var perspective = new Sdk.Perspective(newPerspective.PerspectiveSequenceNodeCid, new PerspectiveQueries(newPerspective.PerspectiveSequenceNodeCid, _persistence));
            var reflection = new Reflection(decisionSequenceCid, query: null);
            var decision = _strategy(reflection, perspective); // evaluating a strategy runs all required queries
            var timestamp = DateTime.UtcNow;

            if (decision is not Decision.Initiate initiate) {
                // Do no more accounting if the decision is to do nothing.
                continue;
            }

            // Strategy decided to do something.
            // Append this initiative to decision sequence
            decisionSequence = DecisionSequenceHead.NewInitiative(new DecisionSequenceNode(
                actionSet: initiate.Initiatives,
                at: timestamp,
                previous: decisionSequenceCid));

            decisionSequenceCid = await _persistence.Put(decisionSequence);

            // Persist all action instructions and build message to send to execution system.
            var initiations = new List<NewDecision.ActionInitiation>();
            foreach (var initiative in initiate.Initiatives) {
                var actionCid = await _persistence.Put(initiative.Action);

                initiations.Add(new NewDecision.ActionInitiation() {
                    ActionType = initiative.Type,
                    ActionCid = actionCid
                });
            }

            await _output.Writer.WriteAsync(new NewDecision(decisionSequenceCid));
        }
    }
}
