using System.Runtime.CompilerServices;
using AskFi.Runtime.Persistence;
using AskFi.Runtime.Queries;
using AskFi.Runtime.Session.Messages;
using static AskFi.Runtime.DataModel;
using static AskFi.Sdk;

namespace AskFi.Runtime.Modules.Strategy;

internal class StrategyController
{
    private readonly IPerspectiveSource _perspectiveSource;
    private readonly Func<Reflection, Perspective, Decision> _strategy;
    private readonly IdeaStore _ideaStore;

    public StrategyController(
        IPerspectiveSource perspectiveSource,
        Func<Reflection, Perspective, Decision> strategy,
        IdeaStore ideaStore)
    {
        _perspectiveSource = perspectiveSource;
        _strategy = strategy;
        _ideaStore = ideaStore;
    }

    public async IAsyncEnumerable<NewActionDecision> Run([EnumeratorCancellation] CancellationToken sessionShutdown)
    {
        var decisionSequence = DecisionSequenceHead.Start;
        var decisionSequenceCid = await _ideaStore.Store(decisionSequence);

        await foreach (var perspective in _perspectiveSource.StreamPerspectives().WithCancellation(sessionShutdown)) {
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
                perspectiveSequenceHead: perspective.LatestPerspectiveSequenceHead,
                at: timestamp,
                previous: decisionSequenceCid));

            decisionSequenceCid = await _ideaStore.Store(decisionSequence);

            // Persist all action instructions and build message to send to execution system.
            var initiations = new List<NewActionDecision.ActionInitiation>();
            foreach (var initiative in initiate.Initiatives) {
                var actionCid = await _ideaStore.Store(initiative.Action);

                initiations.Add(new NewActionDecision.ActionInitiation() {
                    ActionType = initiative.Type,
                    ActionCid = actionCid
                });
            }

            yield return new NewActionDecision() {
                DecisionSequenceCid = decisionSequenceCid,
                ActionSet = initiations
            };
        }
    }
}
