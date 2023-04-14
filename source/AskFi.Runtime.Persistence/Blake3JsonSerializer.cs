using System.Text;
using AskFi.Persistence;
using Blake3;
using Newtonsoft.Json;

namespace AskFi.Runtime.Persistence;

public class Blake3JsonSerializer : Serializer
{
    public EncodedIdea serialize<TIdea>(TIdea value)
    {
        var json = JsonConvert.SerializeObject(value);
        var bytes = Encoding.UTF8.GetBytes(json);
        var hash = Hasher.Hash(bytes);
        var hashRaw = hash.AsSpanUnsafe().ToArray();

        return new EncodedIdea(
            cid: new ContentId(hashRaw),
            content: bytes);
    }

    public TIdea deserialize<TIdea>(EncodedIdea value)
    {
        var json = Encoding.UTF8.GetString(value.Content);
        var idea = JsonConvert.DeserializeObject<TIdea>(json);

        return idea;
    }
}
