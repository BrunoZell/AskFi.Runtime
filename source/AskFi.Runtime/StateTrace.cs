using AskFi.Persistence;

namespace AskFi.Runtime;

public class StateTrace
{
    public Dictionary<Type, ContentId> LatestObservationSequences { get; } = new();
    public ContentId LatestPerspectiveSequence { get; set; }
}
