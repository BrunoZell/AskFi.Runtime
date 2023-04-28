namespace AskFi.Runtime.Persistence.Caches;

public class DiskCache
{

    private readonly DirectoryInfo _localPersistenceDirectory;

    public DiskCache(DirectoryInfo localPersistenceDirectory) =>
        _localPersistenceDirectory = localPersistenceDirectory;

    public async ValueTask<byte[]?> TryReadFromDisk(ContentId cid)
    {
        var relativePath = BuildFilePath(cid);
        var absolutePath = Path.Combine(_localPersistenceDirectory.FullName, relativePath);

        try {
            // Todo: Handle case when another process is still writing this file and hasn't completed that operation yet. Try to verify the CID after load. Or use file sharing that locks out every read while write.
            var content = await File.ReadAllBytesAsync(absolutePath);

            return content;
        } catch (FileNotFoundException) {
            return null;
        } catch (DirectoryNotFoundException) {
            return null;
        }
    }

    public async ValueTask WriteToDisk(ContentId cid, byte[] raw)
    {
        var relativePath = BuildFilePath(cid);
        var relativeDirectory = Path.GetDirectoryName(relativePath)!;
        _localPersistenceDirectory.CreateSubdirectory(relativeDirectory);

        var absolutePath = Path.Combine(_localPersistenceDirectory.FullName, relativePath);
        await File.WriteAllBytesAsync(absolutePath, raw);
        // Todo: Block all reads until write is fully done.
    }

    private static string BuildFilePath(ContentId contentId)
    {
        var fullPath = Base32.ToBase32String(contentId.Raw);
        return Path.Combine(
            fullPath[0..1],
            fullPath[2..3],
            fullPath[4..]);
    }
}
