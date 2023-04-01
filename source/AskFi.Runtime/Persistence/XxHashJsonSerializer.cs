using System.Text;
using AskFi.Persistence;
using Newtonsoft.Json;
using Standart.Hash.xxHash;

namespace AskFi.Runtime.Persistence;

internal class XxHashJsonSerializer : Serializer
{
    public EncodedIdea serialize<TIdea>(TIdea value)
    {
        var json = JsonConvert.SerializeObject(value);
        var bytes = Encoding.UTF8.GetBytes(json);
        var hash = xxHash128.ComputeHash(bytes, bytes.Length).ToBytes();

        return new EncodedIdea(
            cid: new ContentId(hash),
            content: bytes);
    }

    public TIdea deserialize<TIdea>(EncodedIdea value)
    {
        var json = Encoding.UTF8.GetString(value.Content);
        var idea = JsonConvert.DeserializeObject<TIdea>(json);

        return idea;
    }
}
