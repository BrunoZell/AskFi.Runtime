using AskFi.Persistence;
using AskFi.Runtime.Persistence;

namespace AskFi.Runtime;

public class AnalyzerBuilder
{
    private readonly Dictionary<Type, ContentId> _observationSequences = new();
    private DirectoryInfo? _localPersistenceDirectory;
    private Uri? _ipfsClusterUrl;

    public void WithObservationSequence<TPerception>(ContentId observationSequenceHeadCid)
    {
        _observationSequences.Add(typeof(TPerception), observationSequenceHeadCid);
    }

    public void WithLocalPersistence(string localPersistenceDirectory)
    {
        _localPersistenceDirectory = new(localPersistenceDirectory);
    }

    public void WithIpfsClusterPersistence(string ipfsClusterUrl)
    {
        _ipfsClusterUrl = new(ipfsClusterUrl);
    }

    public Analyzer Build()
    {
        if (_localPersistenceDirectory is null)
        {
            throw new InvalidOperationException("A local persistence path must be specified before building an Analyzer Instance.");
        }

        _localPersistenceDirectory.Create();
        var storageEnvironment = new StorageEnvironment(_localPersistenceDirectory, _ipfsClusterUrl);

        return new Analyzer(_observationSequences, storageEnvironment);
    }
}
