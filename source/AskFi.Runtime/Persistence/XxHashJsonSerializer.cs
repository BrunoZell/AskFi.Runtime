using System.Text;
using System.Text.Json;
using AskFi.Persistence;
using Standart.Hash.xxHash;

namespace AskFi.Runtime.Persistence;

internal class XxHashJsonSerializer : Serializer
{
    public EncodedIdea serialize<TIdea>(TIdea value)
    {
        var json = JsonSerializer.Serialize(value);
        var bytes = Encoding.UTF8.GetBytes(json);
        var hash = xxHash128.ComputeHash(bytes, bytes.Length).ToBytes();

        return new EncodedIdea(
            cid: new ContentId(hash),
            content: bytes);
    }

    public TIdea deserialize<TIdea>(EncodedIdea value) =>
        throw new NotImplementedException();
}
