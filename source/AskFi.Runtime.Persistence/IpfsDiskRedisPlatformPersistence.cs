using AskFi.Runtime.Persistence.Encoding;
using AskFi.Runtime.Persistence.InMemory;
using AskFi.Runtime.Platform;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace AskFi.Runtime.Persistence;

public class IpfsDiskRedisPlatformPersistence : IPlatformPersistence
{
    private readonly ISerializer _serializer;
    private readonly ConnectionMultiplexer _redis;
    private readonly ILogger? _logger;

    private readonly ObjectCache _inMemoryObjectCache = new();

    public IpfsDiskRedisPlatformPersistence(
        ISerializer serializer,
        string redisEndpoint,
        ILogger? logger = null)
    {
        _serializer = serializer;
        _redis = ConnectionMultiplexer.Connect(
            new ConfigurationOptions {
                EndPoints = { redisEndpoint },
            });
        _logger = logger;
    }

    public IpfsDiskRedisPlatformPersistence(
        ISerializer serializer,
        EndPointCollection redisEndpoints,
        ILogger? logger = null)
    {
        _serializer = serializer;
        _redis = ConnectionMultiplexer.Connect(
            new ConfigurationOptions {
                EndPoints = redisEndpoints,
            });
        _logger = logger;
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
        // 3. Try read from IPFS Cluster

        throw new NotImplementedException();
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

        // 2. Broadcast PUT via Redis

        // 3. Write data to disk for persistence

        // 4. Upload to IPFS Cluster
        // Todo: Call IPFS Cluster API

        return cid;
    }
}
