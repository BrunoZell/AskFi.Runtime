using AskFi.Persistence;
using AskFi.Runtime.Persistence;

namespace AskFi.Runtime;

public delegate TResult AnalysisQuery<out TResult>(Sdk.Perspective perspective);

public class AnalyzerBuilder<TResult>
{

    private readonly List<ContentId> _observationSequences = new();
    private AnalysisQuery<TResult>? _analysisQuery;
    private DirectoryInfo? _localPersistenceDirectory;
    private Uri? _ipfsClusterUrl;

    public void WithObservationSequence(ContentId observationSequenceHeadCid)
    {
        _observationSequences.Add(observationSequenceHeadCid);
    }

    public void WithAnalyzer(AnalysisQuery<TResult> analysisQuery)
    {
        if (_analysisQuery is not null) {
            throw new InvalidOperationException(
                $"An analysis query has already been added to this Analyzer Builder. " +
                $"Only one analysis query per instance can be used.");
        }

        _analysisQuery = analysisQuery;
    }

    public void WithLocalPersistence(string localPersistenceDirectory)
    {
        _localPersistenceDirectory = new(localPersistenceDirectory);
    }

    public void WithIpfsClusterPersistence(string ipfsClusterUrl)
    {
        _ipfsClusterUrl = new(ipfsClusterUrl);
    }

    public Analyzer<TResult> Build()
    {
        if (_analysisQuery is null) {
            throw new InvalidOperationException("An analysis query must be specified before building an Analyzer Instance.");
        }

        if (_localPersistenceDirectory is null) {
            throw new InvalidOperationException("A local persistence path must be specified before building an Analyzer Instance.");
        }

        _localPersistenceDirectory.Create();
        var storageEnvironment = new StorageEnvironment(_localPersistenceDirectory, _ipfsClusterUrl);

        return new Analyzer<TResult>(_analysisQuery, storageEnvironment);
    }
}
