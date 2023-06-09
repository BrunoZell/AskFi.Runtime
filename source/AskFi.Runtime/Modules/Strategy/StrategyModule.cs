using System.Threading.Channels;
using AskFi.Runtime.Messages;
using AskFi.Runtime.Modules.Perspective;
using AskFi.Runtime.Platform;
using static AskFi.Runtime.DataModel;
using static AskFi.Sdk;

namespace AskFi.Runtime.Modules.Strategy;

internal class StrategyModule
{
    private readonly Func<Reflection, Sdk.Perspective, Decision> _strategy;
    private readonly IPlatformPersistence _persistence;
    private readonly ChannelReader<NewPerspective> _input;
    private readonly Channel<NewDecision> _output;

    public ChannelReader<NewDecision> Output => _output;

    public StrategyModule(
        Func<Reflection, Sdk.Perspective, Decision> strategy,
        IPlatformPersistence persistence,
        ChannelReader<NewPerspective> input)
    {
        _strategy = strategy;
        _persistence = persistence;
        _input = input;
        _output = Channel.CreateUnbounded<NewDecision>();
    }

    public async Task Run(CancellationToken sessionShutdown)
    {
        var decisionSequence = DecisionSequenceHead.Start;
        var decisionSequenceCid = await _persistence.Put(decisionSequence);

        await foreach (var newPerspective in _input.ReadAllAsync(sessionShutdown)) {
            var perspective = new Sdk.Perspective(query: null);
            var reflection = new Reflection(query: null);
            var decision = _strategy(reflection, perspective); // evaluating a strategy runs all required queries

            if (decision is not Decision.Initiate initiate) {
                // Do no more accounting if the decision is to do nothing.
                continue;
            }

            // Strategy decided to do something.

            // Build action set
            var initiations = new List<DataModel.ActionInitiation>();
            foreach (var initiative in initiate.Initiatives) {
                var actionCid = await _persistence.Put(initiative.Action);
                initiations.Add(new DataModel.ActionInitiation(initiative.Type, actionCid));
            }

            var actionSet = new ActionSet(initiations.ToArray());
            var actionSetCid = await _persistence.Put(actionSet);

            // Append this action set to the decision sequence
            decisionSequence = DecisionSequenceHead.NewInitiative(new DecisionSequenceNode(
                previous: decisionSequenceCid,
                actionSet: actionSetCid));

            decisionSequenceCid = await _persistence.Put(decisionSequence);

            await _output.Writer.WriteAsync(new NewDecision(decisionSequenceCid));
        }
    }
}
