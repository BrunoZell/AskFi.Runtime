using AskFi.Runtime.Persistence.InMemory;

namespace AskFi.Runtime.Persistence;

public interface IStorageEnvironment
{
    public ValueTask<EncodedIdea?> TryLoadFromLocalFile(ContentId contentId);
    public ValueTask<EncodedIdea?> TryLoadFromCluster(ContentId contentId);

    public ValueTask PutInLocalFile(EncodedIdea idea);
    public ValueTask PutInCluster(EncodedIdea idea);
}
