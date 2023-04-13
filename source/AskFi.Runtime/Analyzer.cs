using AskFi.Runtime.Persistence;

namespace AskFi.Runtime;

public class Analyzer<TResult>
{
    private AnalysisQuery<TResult> _analysisQuery;
    private StorageEnvironment _storageEnvironment;

    internal Analyzer(AnalysisQuery<TResult> analysisQuery, StorageEnvironment storageEnvironment)
    {
        _analysisQuery = analysisQuery;
        _storageEnvironment = storageEnvironment;
    }
}
