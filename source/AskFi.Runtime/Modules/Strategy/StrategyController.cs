using System.Runtime.CompilerServices;
using AskFi.Runtime.Platform;
using AskFi.Runtime.Session.Messages;
using static AskFi.Runtime.DataModel;
using static AskFi.Sdk;

namespace AskFi.Runtime.Modules.Strategy;

internal class StrategyController
{
    private readonly Func<Reflection, Sdk.Perspective, Decision> _strategy;
    private readonly IPlatformMessaging _messaging;
    private readonly IPlatformPersistence _persistence;

    public StrategyController(
        Func<Reflection, Sdk.Perspective, Decision> strategy,
        IPlatformMessaging messaging,
        IPlatformPersistence persistence)
    {
        _strategy = strategy;
        _messaging = messaging;
        _persistence = persistence;
    }

    public async IAsyncEnumerable<NewDecision> Run([EnumeratorCancellation] CancellationToken sessionShutdown)
    {
        var decisionSequence = DecisionSequenceHead.Start;
        var decisionSequenceCid = await _persistence.Put(decisionSequence);

        await foreach (var perspective in _messaging.StreamPerspectives().WithCancellation(sessionShutdown)) {
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

            yield return new NewDecision() {
                DecisionSequenceCid = decisionSequenceCid,
                ActionSet = initiations
            };
        }
    }
}
