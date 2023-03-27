using System.Collections.Concurrent;
using static AskFi.Runtime.DataModel;

namespace AskFi.Runtime.Queries;

internal static class PerspectiveSequenceStore
{
    private static readonly ConcurrentDictionary<int, PerspectiveSequenceHead> History = new();

    /// <inheritdoc cref="LookupSequencePosition(Sdk.Perspective)"/>
    internal static PerspectiveSequenceHead LookupSequencePosition(int perspectiveSequenceHeadHash)
    {
        return History[perspectiveSequenceHeadHash];
    }

    /// <summary>
    /// Looks up the underlying <see cref="PerspectiveSequenceHead"/> behind the passed <paramref name="perspective"/>.
    /// Throws if the <see cref="PerspectiveSequenceHead"/> has not been inserted via <see cref="Store(PerspectiveSequenceHead)"/> first.
    /// </summary>
    internal static PerspectiveSequenceHead LookupSequencePosition(Sdk.Perspective perspective) =>
        LookupSequencePosition(perspective.HashOfLatestPerspectiveSequenceHead);

    /// <summary>
    /// Called by <see cref="Behavior.PerspectiveSequencer"/> on every new observation.
    /// </summary>
    /// <param name="perspectiveSequence">The latest <see cref="PerspectiveSequenceHead"/> with the new observation as its head.</param>
    internal static PerspectiveHash Store(PerspectiveSequenceHead perspectiveSequence)
    {
        var rawHash = perspectiveSequence.GetHashCode();
        History.TryAdd(rawHash, perspectiveSequence);
        var hash = PerspectiveHash.NewHash(rawHash);
        return hash;
    }
}
