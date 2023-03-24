using System.Collections.Concurrent;
using static AskFi.Runtime.DataModel;

namespace AskFi.Runtime.Queries;

internal static class WorldEventStore
{
    private static readonly ConcurrentDictionary<int, PerspectiveSequenceHead> History = new();

    /// <inheritdoc cref="LookupSequencePosition(Sdk.WorldState)"/>
    internal static PerspectiveSequenceHead LookupSequencePosition(int perspectiveSequenceHeadHash)
    {
        return History[perspectiveSequenceHeadHash];
    }

    /// <summary>
    /// Looks up the underlying <see cref="PerspectiveSequenceHead"/> behind the passed <paramref name="worldState"/>.
    /// Throws if the <see cref="PerspectiveSequenceHead"/> has not been inserted via <see cref="Store(PerspectiveSequenceHead)"/> first.
    /// </summary>
    internal static PerspectiveSequenceHead LookupSequencePosition(Sdk.WorldState worldState) =>
        LookupSequencePosition(worldState.HashOfLatestPerspectiveSequenceHead);

    /// <summary>
    /// Called by <see cref="Behavior.PerspectiveSequencer"/> on every new observation.
    /// </summary>
    /// <param name="eventSequence">The latest <see cref="PerspectiveSequenceHead"/> with the new observation as its head.</param>
    internal static int Store(PerspectiveSequenceHead eventSequence)
    {
        var hash = eventSequence.GetHashCode();
        History.TryAdd(hash, eventSequence);
        return hash;
    }
}
