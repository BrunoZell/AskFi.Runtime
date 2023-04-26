using System.Collections.Concurrent;

namespace AskFi.Runtime.Persistence;

public sealed class IdeaStore
{
    private readonly ISerializer _defaultSerializer;
    private readonly IStorageEnvironment _storageEnvironment;
    private readonly ConcurrentDictionary<ContentId, IdeaStorageCell> Index = new();

    public IdeaStore(ISerializer defaultSerializer, IStorageEnvironment storageEnvironment)
    {
        _defaultSerializer = defaultSerializer;
        _storageEnvironment = storageEnvironment;
    }

    public ValueTask<TIdea> Load<TIdea>(ContentId contentId)
    {
        var cell = GetStorageCell(contentId);
        return cell.Load<TIdea>(_defaultSerializer);
    }

    private IdeaStorageCell GetStorageCell(ContentId contentId)
    {
        if (Index.TryGetValue(contentId, out var storageCell)) {
            return storageCell;
        }

        return Index[contentId] = new(contentId, _storageEnvironment);
    }

    /// <summary>
    /// Persist an idea in a content-addressable way.
    /// </summary>
    public async ValueTask<ContentId> Store<TIdea>(TIdea idea)
    {
        var encoded = _defaultSerializer.Serialize(idea);
        var cell = Index[encoded.Cid] = new(encoded.Cid, _storageEnvironment);

        cell.SetCache(idea);
        await _storageEnvironment.PutInLocalFile(encoded);

        return encoded.Cid;
    }
}
