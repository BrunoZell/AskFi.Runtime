using AskFi.Runtime.Persistence.InMemory;

namespace AskFi.Runtime.Persistence.Encoding;

public interface ISerializer
{
    EncodedIdea Serialize<TIdea>(TIdea idea);
    TIdea Deserialize<TIdea>(EncodedIdea encodedIdea);
}
