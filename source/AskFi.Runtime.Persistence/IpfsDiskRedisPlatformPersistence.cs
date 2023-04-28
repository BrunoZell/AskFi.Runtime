using AskFi.Runtime.Persistence.Encoding;
using AskFi.Runtime.Platform;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace AskFi.Runtime.Persistence;

public class IpfsDiskRedisPlatformPersistence : IPlatformPersistence
{
    private readonly ISerializer _serializer;
    private readonly ConnectionMultiplexer _redis;
    private readonly ILogger? _logger;

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

    public ContentId Cid<TDatum>(TDatum value)
    {
        // Generate cid locally, insert in in-memory cid->obj mapping
        throw new NotImplementedException();
    }

    public ValueTask<TDatum> Get<TDatum>(ContentId value)
    {
        // 1. Try read from in-memory cid->obj mapping
        // 2. Try read from local disk
        // 3. Try read from IPFS Cluster

        throw new NotImplementedException();
    }

    public ValueTask<bool> Pin(ContentId value)
    {
        // Todo: Pin in IPFS Cluster
        return new(false);
    }

    public ValueTask<ContentId> Put<TDatum>(TDatum value)
    {
        // 1. Generate cid locally, insert in in-memory cid->obj mapping
        // 2. Broadcast PUT via Redis
        // 3. Write data to disk for persistence
        // 4. Upload to IPFS Cluster

        throw new NotImplementedException();
    }
}
