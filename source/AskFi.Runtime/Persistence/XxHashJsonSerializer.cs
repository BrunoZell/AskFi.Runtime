using System.Text;
using System.Text.Json;
using Microsoft.FSharp.Core;
using Standart.Hash.xxHash;
using static AskFi.Sdk;

namespace AskFi.Runtime.Persistence;

public class XxHashJsonSerializer : Serializer
{
    public EncodedIdea serialize<TIdea>(TIdea value)
    {
        var json = JsonSerializer.Serialize(value);
        var bytes = Encoding.UTF8.GetBytes(json);
        var hash = xxHash128.ComputeHash(bytes, bytes.Length).ToBytes();

        return new EncodedIdea(
            cid: ContentId.NewContentId(hash),
            content: bytes);
    }

    public FSharpOption<TIdea> deserialize<TIdea>(EncodedIdea value) =>
        throw new NotImplementedException();
}
