using System.Collections.Immutable;
using AskFi.Runtime.Observation;
using AskFi.Runtime.Persistence;

namespace AskFi.Runtime;

public class Scraper
{
    private readonly IReadOnlyDictionary<Type, object> _observers;
    private readonly StorageEnvironment _storageEnvironment;

    internal Scraper(IReadOnlyDictionary<Type, object> observers, StorageEnvironment storageEnvironment)
    {
        _observers = observers;
        _storageEnvironment = storageEnvironment;
    }

    public async Task Run(CancellationToken sessionShutdown)
    {
        await Task.Yield();

        var ideaStore = new IdeaStore(defaultSerializer: new Blake3JsonSerializer(), _storageEnvironment);
        var observerGroup = new ObserverGroup(ideaStore);
        var observerInstances = _observers
            .Select(o => ObserverInstance.StartNew(o.Key, o.Value, observerGroup.ObservationSink, ideaStore, sessionShutdown))
            .ToImmutableList();

        await foreach (var perspective in observerGroup.Sequence()) {
            // Todo: Index in etcd
            // Todo: publish in rabot:new-observation
        }
    }
}
