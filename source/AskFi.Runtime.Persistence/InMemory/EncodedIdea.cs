namespace AskFi.Runtime.Persistence.InMemory;

public class EncodedIdea
{
    /// <summary>
    /// Content-id of the idea. This is a hash of this ideas 'Content' and is used throughout
    /// the system to uniquely identify this idea in a content-addressed manner.
    /// </summary>
    public required ContentId Cid { get; init; }

    /// <summary>
    /// Content-id of the idea. This is a hash of this ideas 'Content' and is used throughout
    /// the system to uniquely identify this idea in a content-addressed manner.
    /// </summary>
    public required byte[] Content { get; init; }
}
