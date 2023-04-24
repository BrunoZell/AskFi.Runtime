namespace AskFi.Runtime.Persistence;

public interface ISerializer
{
    EncodedIdea Serialize<TIdea>(TIdea idea);
    TIdea Deserialize<TIdea>(EncodedIdea encodedIdea);
}
