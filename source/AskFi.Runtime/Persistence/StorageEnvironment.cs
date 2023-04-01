using AskFi.Persistence;

namespace AskFi.Runtime.Persistence;

internal class StorageEnvironment : IStorageEnvironment
{
    private readonly DirectoryInfo _localStoragePath;
    private readonly Uri? _ipfsClusterUrl;

    public StorageEnvironment(DirectoryInfo localStoragePath, Uri? ipfsClusterUrl = null)
    {
        _localStoragePath = localStoragePath;
        _ipfsClusterUrl = ipfsClusterUrl;
    }

    #region Local Disk
    public async ValueTask<EncodedIdea?> TryLoadFromLocalFile(ContentId contentId)
    {
        var relativePath = BuildFilePath(contentId);
        var absolutePath = Path.Combine(_localStoragePath.FullName, relativePath);

        try
        {
            var content = await File.ReadAllBytesAsync(absolutePath);
            return new EncodedIdea(contentId, content);
        }
        catch (FileNotFoundException)
        {
            return null;
        }
        catch (DirectoryNotFoundException)
        {
            return null;
        }
    }

    public async ValueTask PutInLocalFile(EncodedIdea idea)
    {
        var relativePath = BuildFilePath(idea.Cid);
        var relativeDirectory = Path.GetDirectoryName(relativePath)!;
        _localStoragePath.CreateSubdirectory(relativeDirectory);

        var absolutePath = Path.Combine(_localStoragePath.FullName, relativePath);
        await File.WriteAllBytesAsync(absolutePath, idea.Content.ToArray());
    }

    private string BuildFilePath(ContentId contentId)
    {
        var fullPath = Base32.ToBase32String(contentId.Raw);
        return Path.Combine(
            fullPath[0..1],
            fullPath[2..3],
            fullPath[4..]);
    }
    #endregion

    #region IPFS Cluster
    public ValueTask<EncodedIdea?> TryLoadFromCluster(ContentId contentId)
    {
        throw new NotImplementedException();
    }

    public ValueTask PutInCluster(EncodedIdea idea)
    {
        throw new NotImplementedException();
    }
    #endregion
}
