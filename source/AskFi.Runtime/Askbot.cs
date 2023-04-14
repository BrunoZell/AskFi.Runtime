using System.Collections.Immutable;
using AskFi.Runtime.Behavior;
using AskFi.Runtime.Observation;
using AskFi.Runtime.Persistence;
using AskFi.Runtime.Queries;
using Newtonsoft.Json;
using static AskFi.Sdk;

namespace AskFi.Runtime;

public class Askbot
{
    private readonly IPerspectiveSource _perspectiveSource;
    private readonly IReadOnlyDictionary<Type, object> _brokers;
    private readonly Func<StrategyReflection, Perspective, Decision> _strategy;
    private readonly IStorageEnvironment _storageEnvironment;

    internal Askbot(
        IPerspectiveSource perspectiveSource,
        IReadOnlyDictionary<Type, object> brokers,
        Func<StrategyReflection, Perspective, Decision> strategy,
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

        var sessionController = new SessionController(_perspectiveSource, _strategy, _brokers);
        await sessionController.Run(sessionShutdown);
    }
}
