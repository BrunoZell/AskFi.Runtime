using AskFi.Runtime.Messages;
using AskFi.Runtime.Persistence.Caching;
using AskFi.Runtime.Persistence.Encoding;
using AskFi.Runtime.Platform;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace AskFi.Runtime.Persistence;

public class IpfsDiskRedisPlatformPersistence : IPlatformPersistence
{
    private readonly ISerializer _serializer;
    private readonly ConnectionMultiplexer _redis;

    private readonly ObjectCache _inMemoryObjectCache = new();
    private readonly DiskCache _diskCache;

    public IpfsDiskRedisPlatformPersistence(
        ISerializer serializer,
        DirectoryInfo localPersistenceDirectory,
        string redisEndpoint)
    {
        _serializer = serializer;
        _diskCache = new(localPersistenceDirectory);
        _redis = ConnectionMultiplexer.Connect(
            new ConfigurationOptions {
                EndPoints = { redisEndpoint },
            });
    }

    public IpfsDiskRedisPlatformPersistence(
        ISerializer serializer,
        DirectoryInfo localPersistenceDirectory,
        EndPointCollection redisEndpoints)
    {
        _serializer = serializer;
        _diskCache = new(localPersistenceDirectory);
        _redis = ConnectionMultiplexer.Connect(
            new ConfigurationOptions {
                EndPoints = redisEndpoints,
            });
    }

    public ContentId Cid<TDatum>(TDatum datum)
    {
        // Generate CID locally
        var (cid, raw) = _serializer.Serialize(datum);

        // Insert into in-memory cid->obj mapping for future GET requests on that CID.
        _inMemoryObjectCache.Set(cid, datum);

        return cid;
    }

    public async ValueTask<TDatum> Get<TDatum>(ContentId cid)
    {
        // 1. Try read from in-memory cid->obj mapping
        if (_inMemoryObjectCache.TryGet(cid, out var c) && c is TDatum cached) {
            return cached;
        }

        // 2. Try read from local disk
        var fromDisk = await _diskCache.TryReadFromDisk(cid);
        if (fromDisk is not null) {
            // Deserialize loaded raw data
            var datum = _serializer.Deserialize<TDatum>(cid, fromDisk);

            // Insert into in-memory cid->obj mapping for future GET requests on that CID.
            _inMemoryObjectCache.Set(cid, datum);

            return datum;
        }

        // 3. Try read from IPFS Cluster

        throw new NotImplementedException("Reading from IPFS Cluster is not yet implemented.");
    }

    public ValueTask<bool> Pin(ContentId cid)
    {
        // Todo: Pin in IPFS Cluster
        return new(false);
    }

    public async ValueTask<ContentId> Put<TDatum>(TDatum datum)
    {
        // 1. Generate CID and raw bytes locally
        var (cid, raw) = _serializer.Serialize(datum);

        // 2. Insert into in-memory cid->obj mapping for future GET requests on that CID.
        _inMemoryObjectCache.Set(cid, datum);

        // 2. Broadcast PUT via Redis (if payload is below 4KB)
        if (raw.Length < 1024 * 4) {
            var publisher = _redis.GetSubscriber();
            var textMessage = JsonConvert.SerializeObject(new PersistencePut(cid, raw));
            publisher.Publish("persistence", textMessage, CommandFlags.FireAndForget);
        }

        // 3. Write data to disk for persistence
        await _diskCache.WriteToDisk(cid, raw);

        // 4. Upload to IPFS Cluster
        // Todo: Call IPFS Cluster API

        return cid;
    }
}
