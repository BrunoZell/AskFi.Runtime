using AskFi.Runtime.Modules.Observation;
using AskFi.Runtime.Persistence;

namespace AskFi.Runtime.Apps;

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
        await using var observerGroup = ObserverGroup.StartNew(_observers, ideaStore, sessionShutdown);

        try {
            // Wait until shutdown got requested through cancellation token.
            await Task.Delay(0, sessionShutdown);
        } catch (OperationCanceledException) {
            // ignore cancellation since it's a graceful shotdown and expected.
        }
    }
}
