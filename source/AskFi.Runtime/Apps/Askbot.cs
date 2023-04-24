using AskFi.Runtime.Execution;
using AskFi.Runtime.Modules.Perspective;
using AskFi.Runtime.Persistence;
using AskFi.Runtime.Session;
using AskFi.Runtime.Strategy;
using static AskFi.Sdk;

namespace AskFi.Runtime.Apps;

public class Askbot
{
    private readonly IPerspectiveSource _perspectiveSource;
    private readonly IReadOnlyDictionary<Type, object> _brokers;
    private readonly Func<Reflection, Perspective, Decision> _strategy;
    private readonly IStorageEnvironment _storageEnvironment;

    internal Askbot(
        IPerspectiveSource perspectiveSource,
        IReadOnlyDictionary<Type, object> brokers,
        Func<Reflection, Perspective, Decision> strategy,
        IStorageEnvironment storageEnvironment)
    {
        _perspectiveSource = perspectiveSource;
        _brokers = brokers;
        _strategy = strategy;
        _storageEnvironment = storageEnvironment;
    }

    public async Task Run(CancellationToken sessionShutdown)
    {
        await Task.Yield();

        var ideaStore = new IdeaStore(defaultSerializer: new Blake3JsonSerializer(), _storageEnvironment);
        var strategyController = new StrategyController(_perspectiveSource, _strategy, ideaStore);
        var actionRouter = new ActionRouter(_brokers, ideaStore);

        await foreach (var action in strategyController.Run(sessionShutdown))             actionRouter.Execute(action);
    }
}