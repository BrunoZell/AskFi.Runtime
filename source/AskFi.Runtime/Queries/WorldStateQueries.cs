using System.Diagnostics;
using AskFi.Runtime.Objects;
using static AskFi.Sdk;

namespace AskFi.Runtime.Queries;
public static class WorldStateQueries
{
    public static IEnumerable<ReadOnlyMemory<TPerception>> QueryPerceptions<TPerception>(WorldState worldState, DateTime since)
    {
        var tree = WorldEventStore.LookupSequencePosition(worldState);

        foreach (var observation in Since(tree, since)) {
            if (observation.observation == typeof(TPerception)) {
                //yield return observation.Perceptions;
            }
        }

        yield break;
    }

    private static IEnumerable<DataModel.WorldEventSequence.Happening> Since(DataModel.WorldEventSequence worldEventSequence, DateTime since)
    {
        // This is to buffer all observations that happens after 'since' until the first observation is inspected that came before 'since'.
        // This means that before anything is returned, all requested observations are loaded into memory.
        // Todo: to optimize this, the runtime should eagerly build the reversed linked list and provide an index into all nodes via a timestamp.
        // Then only a tree-node is returned and the iteration of it is on the user.
        var selectedObservations = new List<DataModel.WorldEventSequence.Happening>();

        while (true) {
            if (worldEventSequence is DataModel.WorldEventSequence.Happening happening) {
                if (happening.at > since) {
                    selectedObservations.Add(happening);
                } else {
                    // Found first observation earlier than 'since'.
                    // Stop iteration here because the timestamps can only ever decrease into the pest.
                    break;
                }

                worldEventSequence = WorldEventStore.LookupSequencePosition(happening.previous);
            } else {
                // No more observations (first node of linked list)
                Debug.Assert(worldEventSequence == DataModel.WorldEventSequence.Empty, "WorldEventSequence should have only two union cases: Empty | Happening");
                break;
            }
        }

        return selectedObservations;
    }
}
