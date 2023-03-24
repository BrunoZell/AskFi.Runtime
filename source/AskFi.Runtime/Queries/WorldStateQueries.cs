using System.Diagnostics;
using Microsoft.FSharp.Core;
using static AskFi.Sdk;

namespace AskFi.Runtime.Queries;
public class WorldStateQueries : IWorldState
{
    private readonly WorldState _worldState;

    public WorldStateQueries(WorldState worldState) =>
        _worldState = worldState;

    public FSharpOption<Perception> latest<Perception>()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<TPerception> since<TPerception>(DateTime timestamp)
    {
        var tree = WorldEventStore.LookupSequencePosition(_worldState);

        foreach (var observation in Since(tree, timestamp)) {
            // Only reuturn observations of requested type TPerception
            if (observation.observationStreamHead is DataModel.ObservationSequenceHead<TPerception>.Observation relevantObservation) {
                yield return relevantObservation.Item.Observation;
            }
        }
    }

    IEnumerable<(FSharpOption<TPerception1>, FSharpOption<TPerception2>)> IWorldState.since<TPerception1, TPerception2>(DateTime timestamp)
    {
        throw new NotImplementedException();
    }

    private static IEnumerable<DataModel.PerspectiveSequenceHead.Happening> Since(DataModel.PerspectiveSequenceHead perspectiveSequenceHead, DateTime since)
    {
        // This is to buffer all observations that happens after 'since' until the first observation is inspected that came before 'since'.
        // This means that before anything is returned, all requested observations are loaded into memory.
        // Todo: to optimize this, the runtime should eagerly build the reversed linked list and provide an index into all nodes via a timestamp.
        // Then only a tree-node is returned and the iteration of it is on the user.
        var selectedObservations = new List<DataModel.PerspectiveSequenceHead.Happening>();

        while (true) {
            if (perspectiveSequenceHead is DataModel.PerspectiveSequenceHead.Happening happening) {
                if (happening.at > since) {
                    selectedObservations.Add(happening);
                } else {
                    // Found first observation earlier than 'since'.
                    // Stop iteration here because the timestamps can only ever decrease into the pest.
                    break;
                }

                perspectiveSequenceHead = WorldEventStore.LookupSequencePosition(happening.previous);
            } else {
                // No more observations (first node of linked list)
                Debug.Assert(perspectiveSequenceHead == DataModel.PerspectiveSequenceHead.Empty, "PerspectiveSequenceHead should have only two union cases: Empty | Happening");
                break;
            }
        }

        return selectedObservations;
    }
}
