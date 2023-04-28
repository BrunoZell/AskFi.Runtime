using AskFi.Runtime.Persistence.Encoding;

namespace AskFi.Runtime.Persistence.InMemory;

internal sealed class IdeaStorageCell
{
    private readonly ContentId _contentId;
    private readonly IStorageEnvironment _storageEnvironment;

    /// <summary>
    /// Cache after deserialization
    /// </summary>
    private readonly WeakReference _inMemoryIdea = new(target: null);

    public IdeaStorageCell(ContentId contentId, IStorageEnvironment storageEnvironment)
    {
        _contentId = contentId;
        _storageEnvironment = storageEnvironment;
    }

    public void SetCache(object idea)
    {
        _inMemoryIdea.Target = idea;
    }

    public async ValueTask<TIdea> Load<TIdea>(ISerializer serializer)
    {
        if (_inMemoryIdea.Target is TIdea alive)             return alive;

        // Not in memory. Try to read from disk.

        var disk = await _storageEnvironment.TryLoadFromLocalFile(_contentId);

        if (disk is not null) {
            // Deserialize, set cache, and return
            var idea = serializer.Deserialize<TIdea>(disk);
            _inMemoryIdea.Target = idea;
            return idea;
        }

        // Not on local disk. Try to load from IPFS cluster.
        throw new NotImplementedException();
    }
}
