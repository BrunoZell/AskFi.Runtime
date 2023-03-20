using System.Collections.Immutable;
using AskFi.Runtime.Internal;
using AskFi.Sdk;

namespace AskFi.Runtime;

public class Askbot
{
    private readonly IReadOnlyDictionary<Type, object> _observers;
    private readonly IReadOnlyDictionary<Type, object> _brokers;
    private readonly Func<StrategyReflection, WorldState, Decision> _strategy;

    public Askbot(
        IReadOnlyDictionary<Type, object> observers,
        IReadOnlyDictionary<Type, object> brokers,
        Func<StrategyReflection, WorldState, Decision> strategy)
    {
        _observers = observers;
        _brokers = brokers;
        _strategy = strategy;
    }

    public async Task Run(CancellationToken sessionShutdown)
    {
        await Task.Yield();

        var worldSequencer = new WorldSequencer();
        var observerSequencers = _observers
            .Select(o => StartObserverSequencer(o.Key, o.Value, worldSequencer, sessionShutdown))
            .ToImmutableList();

        var sessionController = new SessionController(worldSequencer, _strategy, _brokers);
        await sessionController.Run(sessionShutdown);
    }

    private static ObserverSequencer StartObserverSequencer(/*P*/ Type perception, /*IObserver<P>*/ object observer, WorldSequencer worldSequencer, CancellationToken sessionShutdown)
    {
        var startNew = typeof(ObserverSequencer).GetMethod(nameof(ObserverSequencer.StartNew))!;
        var startNewP = startNew.MakeGenericMethod(perception);
        var sequencer = (ObserverSequencer)startNewP.Invoke(obj: null, new object[] { observer, worldSequencer.ObservationSink, sessionShutdown });
        return sequencer;
    }
}
