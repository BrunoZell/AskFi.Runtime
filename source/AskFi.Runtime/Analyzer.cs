using AskFi.Persistence;
using AskFi.Runtime.Persistence;
using AskFi.Runtime.Replay;
using static AskFi.Sdk;

namespace AskFi.Runtime;

public class Analyzer
{
    private readonly IReadOnlyDictionary<Type, ContentId> _observationSequences;
    private readonly StorageEnvironment _storageEnvironment;

    internal Analyzer(IReadOnlyDictionary<Type, ContentId> observationSequences, StorageEnvironment storageEnvironment)
    {
        _observationSequences = observationSequences;
        _storageEnvironment = storageEnvironment;

        if (_observationSequences.Count != 1)
        {
            throw new ArgumentException("Retroactive merging of multiple observation sequences is not supported yet.");
        }
    }

    public IAsyncEnumerable<Perspective> Perspectives()
    {
        var ideaStore = new IdeaStore(defaultSerializer: new Blake3JsonSerializer(), _storageEnvironment);

        var (perception, cid) = _observationSequences.First();
        var synthesizer = new PerspectiveSynthesis(perception, cid, ideaStore);

        return synthesizer.Sequence();
    }
}
