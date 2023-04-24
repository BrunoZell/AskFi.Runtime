using System.Reflection;
using AskFi.Persistence;
using AskFi.Runtime.Persistence;
using AskFi.Runtime.Platform;
using AskFi.Runtime.Session.Messages;
using static AskFi.Sdk;

namespace AskFi.Runtime.Modules.Execution;

internal class ActionRouter
{
    private readonly IReadOnlyDictionary<Type, object> _brokers;
    private readonly IPlatformPersistence _persistence;

    public ActionRouter(IReadOnlyDictionary<Type, object> brokers, IPlatformPersistence persistence)
    {
        _brokers = brokers;
        _persistence = persistence;
    }

    public void Execute(NewDecision actionDecision)
    {
        // Assign all action initiations an id and send to according broker instance
        foreach (var initiation in actionDecision.ActionSet) {
            InitiateAction(initiation);
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

    private static async Task ExecuteAction<TAction>(IBroker<TAction> broker, ContentId actionCid, IdeaStore ideaStore)
    {
        // Immediately yields back to ensure runtime does not block while action is executed.
        await Task.Yield();

        try {
            // Load action instructions into memory
            var action = await ideaStore.Load<TAction>(actionCid);

            // Execute action using user-provided IBroker-instance.
            await broker.Execute(action);
        } catch (Exception ex) {
            // Todo: Formally catch those exceptions and expose them via the Runtime Data Model.
            Console.WriteLine(ex.ToString());
            throw;
        }
    }
}
