using System.Collections.Immutable;
using AskFi.Runtime.Behavior;
using AskFi.Runtime.Persistence;
using Newtonsoft.Json;
using static AskFi.Sdk;

namespace AskFi.Runtime;

public class Askbot
{
    private readonly IReadOnlyDictionary<Type, object> _observers;
    private readonly IReadOnlyDictionary<Type, object> _brokers;
    private readonly Func<StrategyReflection, Perspective, Decision> _strategy;
    private readonly IStorageEnvironment _storageEnvironment;
    private readonly StateTrace _stateTrace = new();

    internal Askbot(
        IReadOnlyDictionary<Type, object> observers,
        IReadOnlyDictionary<Type, object> brokers,
        Func<StrategyReflection, Perspective, Decision> strategy,
        IStorageEnvironment storageEnvironment)
    {
        _observers = observers;
        _brokers = brokers;
        _strategy = strategy;
        _storageEnvironment = storageEnvironment;
    }

    public async Task Run(CancellationToken sessionShutdown)
    {
        await Task.Yield();

        var ideaStore = new IdeaStore(defaultSerializer: new Blake3JsonSerializer(), _storageEnvironment);
        var perspectiveSequencer = new PerspectiveSequencer(ideaStore, _stateTrace);
        var observerSequencers = _observers
            .Select(o => ObserverSequencer.StartNew(o.Key, o.Value, perspectiveSequencer.ObservationSink, ideaStore, _stateTrace, sessionShutdown))
            .ToImmutableList();

        _ = PeriodicallyPersistStateTrace(@"state.json", sessionShutdown);

        var sessionController = new SessionController(perspectiveSequencer, _strategy, _brokers);
        await sessionController.Run(sessionShutdown);
    }

    private async Task PeriodicallyPersistStateTrace(string fileName, CancellationToken cancellationToken)
    {
        while (true) {
            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);

            var json = JsonConvert.SerializeObject(_stateTrace, Formatting.Indented);
            File.WriteAllText(fileName, json);
        }
    }
}
