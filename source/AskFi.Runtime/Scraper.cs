using System.Collections.Immutable;
using AskFi.Runtime.Behavior;
using AskFi.Runtime.Persistence;
using Newtonsoft.Json;

namespace AskFi.Runtime;

public class Scraper
{
    private readonly Dictionary<Type, object> _observers;
    private readonly StorageEnvironment _storageEnvironment;
    private readonly StateTrace _stateTrace = new();

    internal Scraper(Dictionary<Type, object> observers, StorageEnvironment storageEnvironment)
    {
        _observers = observers;
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

        await foreach (var perspective in perspectiveSequencer.Sequence()) {
            // Iterate to produce and persist perspectives.
        }
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
