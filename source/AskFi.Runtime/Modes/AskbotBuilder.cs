using AskFi.Runtime.Persistence;
using AskFi.Runtime.Queries;
using StrategyDelegate = System.Func<AskFi.Sdk.Reflection, AskFi.Sdk.Perspective, AskFi.Sdk.Decision>;

namespace AskFi.Runtime.Modes;

public class AskbotBuilder
{
    private IPerspectiveSource? _perspectiveSource;
    private readonly Dictionary<Type, object> _brokers = new(); // of type IBroker<Action> (where Action = .Key)
    private StrategyDelegate? _strategy;
    private DirectoryInfo? _localPersistenceDirectory;
    private Uri? _ipfsClusterUrl;

    public void WithPerspective(IPerspectiveSource perspectiveSource)
    {
        if (_perspectiveSource is not null) {
            throw new InvalidOperationException(
                $"A Perspective Source already has been added to this Askbot Builder." +
                $"Only one Perspective Source per Askbot Instance can be used.");
        }

        _perspectiveSource = perspectiveSource;
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

        return new Askbot(_perspectiveSource, _brokers, _strategy, storageEnvironment);
    }
}
