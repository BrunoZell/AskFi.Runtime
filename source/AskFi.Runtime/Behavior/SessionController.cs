using static AskFi.Sdk;

namespace AskFi.Runtime.Behavior;

internal class SessionController
{
    private readonly PerspectiveSequencer _perspectiveSequencer;
    private readonly Func<StrategyReflection, WorldState, Decision> _strategy;
    private readonly IReadOnlyDictionary<Type, object> _brokers;

    public SessionController(
        PerspectiveSequencer perspectiveSequencer,
        Func<StrategyReflection, WorldState, Decision> strategy,
        IReadOnlyDictionary<Type, object> brokers)
    {
        _perspectiveSequencer = perspectiveSequencer;
        _strategy = strategy;
        _brokers = brokers;
    }

    public async Task Run(CancellationToken sessionShutdown)
    {
        var initiatedActions = new HashSet<ActionId>();

        await foreach (var perspective in _perspectiveSequencer.Sequence().WithCancellation(sessionShutdown)) {
            var reflection = new StrategyReflection(initiatedActions.ToArray());
            var decision = _strategy(reflection, perspective); // evaluating a strategy runs all required queries

            if (decision is Decision.Initiate initiate) {
                // Strategy decided to do something.
                // Assign all action initiations an id and send to according broker instance
                foreach (var action in initiate.ActionSet.Initiatives.ToArray()) {
                    var actionId = ActionId.NewActionId(DateTime.UtcNow, _nonce: 0ul); // Todo: Ensure uniqeness
                    initiatedActions.Add(actionId);
                    ExecuteAction(actionId, action);
                }
            }
        }
    }

    private void ExecuteAction(ActionId actionId, ActionInitiation actionInitiation)
    {
        if (!_brokers.TryGetValue(actionInitiation.Type, out var broker)) {
            throw new InvalidOperationException("No broker available that can handle this type of action");
        }

        GenericExecute((dynamic)broker, actionId, actionInitiation.Action);
    }

    private static void GenericExecute<TAction>(IBroker<TAction> broker, ActionId actionId, TAction action)
    {
        // Broker implementation should be non-blocking
        // Todo: Maybe start in an async task wrapper that immediately yields back to ensure runtime does not block by faulty broker implementations? (i.e. doing synchronous network IO directly in 'Execute').
        broker.Execute(actionId, action);
    }
}
