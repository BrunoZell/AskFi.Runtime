using AskFi.Runtime.Persistence;
using static AskFi.Runtime.DataModel;

namespace AskFi.Runtime.Queries;

internal class PerspectiveSequenceStore
{
    private readonly IdeaStore _store;

    public PerspectiveSequenceStore(IdeaStore store)
    {
        _store = store;
    }

    /// <inheritdoc cref="LookupSequencePosition(Sdk.Perspective)"/>
    public ValueTask<PerspectiveSequenceHead> LookupSequencePosition(Sdk.ContentId perspectiveSequenceHeadCid)
    {
        return _store.Load<PerspectiveSequenceHead>(perspectiveSequenceHeadCid);
    }

    /// <summary>
    /// Looks up the underlying <see cref="PerspectiveSequenceHead"/> behind the passed <paramref name="perspective"/>.
    /// Throws if the <see cref="PerspectiveSequenceHead"/> has not been inserted via <see cref="Store(PerspectiveSequenceHead)"/> first.
    /// </summary>
    public ValueTask<PerspectiveSequenceHead> LookupSequencePosition(Sdk.Perspective perspective) =>
        LookupSequencePosition(perspective.LatestPerspectiveSequenceHead);

    /// <summary>
    /// Called by <see cref="Behavior.PerspectiveSequencer"/> on every new observation.
    /// </summary>
    /// <param name="perspectiveSequence">The latest <see cref="PerspectiveSequenceHead"/> with the new observation as its head.</param>
    public ValueTask<Sdk.ContentId> Store(PerspectiveSequenceHead perspectiveSequence)
    {
        return _store.Store(perspectiveSequence);
    }
}
