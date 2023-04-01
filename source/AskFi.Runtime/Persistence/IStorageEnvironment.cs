using AskFi.Persistence;

namespace AskFi.Runtime.Persistence;

internal interface IStorageEnvironment
{
    public ValueTask<EncodedIdea?> TryLoadFromLocalFile(ContentId contentId);
    public ValueTask<EncodedIdea?> TryLoadFromCluster(ContentId contentId);

    public ValueTask PutInLocalFile(EncodedIdea idea);
    public ValueTask PutInCluster(EncodedIdea idea);
}
