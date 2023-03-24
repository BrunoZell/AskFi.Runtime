using System.Collections.Concurrent;
using static AskFi.Runtime.DataModel;

namespace AskFi.Runtime.Queries;

internal static class WorldEventStore
{
    private static readonly ConcurrentDictionary<int, WorldEventSequence> History = new();

    /// <inheritdoc cref="LookupSequencePosition(Sdk.WorldState)"/>
    internal static WorldEventSequence LookupSequencePosition(int worldEventSequenceHash)
    {
        return History[worldEventSequenceHash];
    }

    /// <summary>
    /// Looks up the underlying <see cref="WorldEventSequence"/> behind the passed <paramref name="worldState"/>.
    /// Throws if the <see cref="WorldEventSequence"/> has not been inserted via <see cref="Store(WorldEventSequence)"/> first.
    /// </summary>
    internal static WorldEventSequence LookupSequencePosition(Sdk.WorldState worldState) =>
        LookupSequencePosition(worldState.HashOfLatestWorldEventSequence);

    /// <summary>
    /// Called by <see cref="Behavior.PerspectiveSequencer"/> on every new observation.
    /// </summary>
    /// <param name="eventSequence">The latest <see cref="WorldEventSequence"/> with the new observation as its head.</param>
    internal static int Store(WorldEventSequence eventSequence)
    {
        var hash = eventSequence.GetHashCode();
        History.TryAdd(hash, eventSequence);
        return hash;
    }
}
