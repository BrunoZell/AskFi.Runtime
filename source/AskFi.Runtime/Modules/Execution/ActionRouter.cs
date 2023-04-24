using System.Reflection;
using AskFi.Persistence;
using AskFi.Runtime.Messages;
using AskFi.Runtime.Platform;
using static AskFi.Sdk;

namespace AskFi.Runtime.Modules.Execution;

internal class ActionRouter
{
    private readonly IReadOnlyDictionary<Type, object> _brokers;
    private readonly IPlatformMessaging _messaging;
    private readonly IPlatformPersistence _persistence;

    public ActionRouter(
        IReadOnlyDictionary<Type, object> brokers,
        IPlatformMessaging messaging,
        IPlatformPersistence persistence)
    {
        _brokers = brokers;
        _messaging = messaging;
        _persistence = persistence;
    }

    public async Task Run(CancellationToken cancellationToken)
    {
        await foreach (var decision in _messaging.Listen<NewDecision>(cancellationToken)) {
            // Assign all action initiations an id and send to according broker instance
            foreach (var initiation in decision.ActionSet) {
                InitiateAction(initiation);
            }
        }
    }

    private void InitiateAction(NewDecision.ActionInitiation initiation)
    {
        if (!_brokers.TryGetValue(initiation.ActionType, out var broker)) {
            throw new InvalidOperationException("No broker available that can handle this type of action");
        }

        // Uses reflection over dynamic to support brokers that implement multiple IBroker<A> interfaces.
        var initiate = typeof(ActionRouter).GetMethod(nameof(ExecuteAction), BindingFlags.Static | BindingFlags.NonPublic)!;
        var initiateA = initiate.MakeGenericMethod(initiation.ActionType);
        _ = initiateA.Invoke(obj: null, new object[] { broker, initiation.ActionCid, _persistence }) as Task;
    }

    private static async Task ExecuteAction<TAction>(IBroker<TAction> broker, ContentId actionCid, IPlatformPersistence persistence)
    {
        // Immediately yields back to ensure runtime does not block while action is executed.
        await Task.Yield();

        try {
            // Load action instructions into memory
            var action = await persistence.Get<TAction>(actionCid);

            // Execute action using user-provided IBroker-instance.
            await broker.Execute(action);
        } catch (Exception ex) {
            // Todo: Formally catch those exceptions and expose them via the Runtime Data Models Action Trace.
            Console.WriteLine(ex.ToString());
            throw;
        }
    }
}
