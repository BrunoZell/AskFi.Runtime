using System.Diagnostics;
using Microsoft.FSharp.Core;
using static AskFi.Sdk;

namespace AskFi.Runtime.Queries;

public sealed class PerspectiveQueries : IPerspectiveQueries
{
    private readonly int _latestPerspectiveSequenceHash;

    public PerspectiveQueries(int latestPerspectiveSequenceHash) =>
        _latestPerspectiveSequenceHash = latestPerspectiveSequenceHash;

    public FSharpOption<Observation<TPerception>> latest<TPerception>()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Observation<TPerception>> since<TPerception>(DateTime timestamp)
    {
        var tree = PerspectiveSequenceStore.LookupSequencePosition(_latestPerspectiveSequenceHash);

        foreach (var happening in LatestObservationTreeHeadsSince(tree, timestamp)) {
            // Only return observations of requested type TPerception
            if (happening.observationStreamHead is DataModel.ObservationSequenceHead<TPerception>.Observation relevantObservation) {
                yield return relevantObservation.Item.Observation;
            }
        }
    }

    public IEnumerable<(FSharpOption<Observation<TPerception1>>, FSharpOption<Observation<TPerception2>>)> since<TPerception1, TPerception2>(DateTime timestamp)
    {
        throw new NotImplementedException();
    }

    private static IEnumerable<DataModel.PerspectiveSequenceHead.Happening> LatestObservationTreeHeadsSince(DataModel.PerspectiveSequenceHead perspectiveSequenceHead, DateTime since)
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

                perspectiveSequenceHead = PerspectiveSequenceStore.LookupSequencePosition(happening.previous);
            } else {
                // No more observations (first node of linked list)
                Debug.Assert(perspectiveSequenceHead == DataModel.PerspectiveSequenceHead.Empty, "PerspectiveSequenceHead should have only two union cases: Empty | Happening");
                break;
            }
        }

        return selectedObservations;
    }
}
