using System.Text;
using AskFi.Runtime.Persistence.InMemory;
using Blake3;
using Newtonsoft.Json;

namespace AskFi.Runtime.Persistence.Encoding;

public class Blake3JsonSerializer : ISerializer
{
    public EncodedIdea Serialize<TIdea>(TIdea value)
    {
        var json = JsonConvert.SerializeObject(value);
        var bytes = Encoding.UTF8.GetBytes(json);
        var hash = Hasher.Hash(bytes);
        var hashRaw = hash.AsSpanUnsafe().ToArray();

        return new EncodedIdea() {
            Cid = new ContentId(hashRaw),
            Content = bytes
        };
    }

    public TIdea Deserialize<TIdea>(EncodedIdea value)
    {
        var json = Encoding.UTF8.GetString(value.Content);
        var idea = JsonConvert.DeserializeObject<TIdea>(json);

        return idea;
    }
}
