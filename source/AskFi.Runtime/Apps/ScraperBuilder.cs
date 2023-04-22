using AskFi.Runtime.Persistence;

namespace AskFi.Runtime.Apps;

public class ScraperBuilder
{
    private readonly Dictionary<Type, object> _observers = new(); // of type IObserver<Perception> (where Perception = .Key)
    private DirectoryInfo? _localPersistenceDirectory;
    private Uri? _ipfsClusterUrl;

    public void AddObserver<TPerception>(Sdk.IObserver<TPerception> observer)
    {
        var added = _observers.TryAdd(typeof(TPerception), observer);

        if (!added)             throw new InvalidOperationException(
                $"An observer for perception type '{typeof(TPerception).FullName}' has already been added to " +
                $"this Askbot Builder. Only one observer instance per perception-type can be used.");
    }

    public void WithLocalPersistence(string localPersistenceDirectory)
    {
        _localPersistenceDirectory = new(localPersistenceDirectory);
    }

    public void WithIpfsClusterPersistence(string ipfsClusterUrl)
    {
        _ipfsClusterUrl = new(ipfsClusterUrl);
    }

    public Scraper Build()
    {
        if (_localPersistenceDirectory is null)             throw new InvalidOperationException("A local persistence path must be specified before building an Askbot Instance.");

        _localPersistenceDirectory.Create();
        var storageEnvironment = new StorageEnvironment(_localPersistenceDirectory, _ipfsClusterUrl);

        return new Scraper(_observers, storageEnvironment);
    }
}
