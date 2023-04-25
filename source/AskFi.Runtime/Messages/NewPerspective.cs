using AskFi.Runtime.Persistence;

namespace AskFi.Runtime.Messages;

internal class NewPerspective
{
    /// <summary>
    /// Cid to the latest <see cref="DataModel.PerspectiveSequenceHead"/>.
    /// </summary>
    public ContentId PerspectiveSequenceHeadCid { get; init; }
}
