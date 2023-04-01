using System.Collections.Immutable;
using System.Diagnostics;
using AskFi.Runtime.Behavior;
using AskFi.Runtime.Persistence;
using static AskFi.Sdk;

namespace AskFi.Runtime;

public class Askbot
{
    private readonly IReadOnlyDictionary<Type, object> _observers;
    private readonly IReadOnlyDictionary<Type, object> _brokers;
    private readonly Func<StrategyReflection, Perspective, Decision> _strategy;
    private readonly IStorageEnvironment _storageEnvironment;

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
        var perspectiveSequencer = new PerspectiveSequencer(ideaStore);
        var observerSequencers = _observers
            .Select(o => StartObserverSequencer(o.Key, o.Value, perspectiveSequencer, ideaStore, sessionShutdown))
            .ToImmutableList();

        var sessionController = new SessionController(perspectiveSequencer, _strategy, _brokers);
        await sessionController.Run(sessionShutdown);
    }

    private static ObserverSequencer StartObserverSequencer(/*'P*/ Type perception, /*IObserver<'P>*/ object observer, PerspectiveSequencer worldSequencer, IdeaStore ideaStore, CancellationToken sessionShutdown)
    {
        var startNew = typeof(ObserverSequencer).GetMethod(nameof(ObserverSequencer.StartNew))!;
        var startNewP = startNew.MakeGenericMethod(perception);
        var sequencer = startNewP.Invoke(obj: null, new object[] { observer, worldSequencer.ObservationSink, ideaStore, sessionShutdown }) as ObserverSequencer;
        Debug.Assert(sequencer is not null, $"Return type of {nameof(ObserverSequencer.StartNew)} changed and now is incompatible with this code.");
        return sequencer;
    }
}
