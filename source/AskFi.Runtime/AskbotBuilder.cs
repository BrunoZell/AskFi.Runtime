using AskFi.Runtime.Persistence;
using StrategyDelegate = System.Func<AskFi.Sdk.Reflection, AskFi.Sdk.Perspective, AskFi.Sdk.Decision>;

namespace AskFi.Runtime;

public class AskbotBuilder
{
    private readonly Dictionary<Type, object> _observers = new(); // of type IObserver<Perception> (where Perception = .Key)
    private readonly Dictionary<Type, object> _brokers = new(); // of type IBroker<Action> (where Action = .Key)
    private StrategyDelegate? _strategy;
    private DirectoryInfo? _localPersistenceDirectory;
    private Uri? _ipfsClusterUrl;

    public void AddObserver<TPerception>(Sdk.IObserver<TPerception> observer)
    {
        var added = _observers.TryAdd(typeof(TPerception), observer);

        if (!added) {
            throw new InvalidOperationException(
                $"An observer for perception type '{typeof(TPerception).FullName}' has already been added to " +
                $"this Askbot Builder. Only one observer instance per perception-type can be used.");
        }
    }

    public void AddBroker<TAction>(Sdk.IBroker<TAction> broker)
    {
        var added = _brokers.TryAdd(typeof(TAction), broker);

        if (!added) {
            throw new InvalidOperationException(
                $"A broker for action type '{typeof(TAction).FullName}' has already been added to " +
                $"this Askbot Builder. Only one broker instance per action-type can be used.");
        }
    }

    public void WithStrategy(StrategyDelegate strategy)
    {
        if (_strategy is not null) {
            throw new InvalidOperationException(
                $"A strategy has already been added to this Askbot Builder. " +
                $"Only one strategy per instance can be used.");
        }

        _strategy = strategy;
    }

    /// <summary>
    /// Configures a strategy that always decides to do nothing (<see cref="Sdk.Decision.Inaction"/>).
    /// </summary>
    public void WithoutStrategy()
    {
        if (_strategy is not null) {
            throw new InvalidOperationException(
                $"A strategy has already been added to this Askbot Builder. " +
                $"Only one strategy per instance can be used.");
        }

        _strategy = (s, w) => Sdk.Decision.Inaction;
    }

    public void WithLocalPersistence(string localPersistenceDirectory)
    {
        _localPersistenceDirectory = new(localPersistenceDirectory);
    }

    public void WithIpfsClusterPersistence(string ipfsClusterUrl)
    {
        _ipfsClusterUrl = new(ipfsClusterUrl);
    }

    public Askbot Build()
    {
        if (_strategy is null) {
            throw new InvalidOperationException("A strategy must be specified before building an Askbot Instance. " +
                $"If no actions ever should be executed, explicitly configure it by calling {nameof(WithoutStrategy)} on the builder.");
        }

        if (_localPersistenceDirectory is null) {
            throw new InvalidOperationException("A local persistence path must be specified before building an Askbot Instance.");
        }

        _localPersistenceDirectory.Create();
        var storageEnvironment = new StorageEnvironment(_localPersistenceDirectory, _ipfsClusterUrl);

        return new Askbot(_observers, _brokers, _strategy, storageEnvironment);
    }
}
