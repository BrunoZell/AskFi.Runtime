using System.Diagnostics;
using AskFi.Persistence;
using AskFi.Runtime.Internal;
using AskFi.Runtime.Platform;
using Microsoft.FSharp.Core;
using static AskFi.Runtime.DataModel;
using static AskFi.Sdk;

namespace AskFi.Runtime.Modules.Perspective;

internal sealed class PerspectiveQueries : IPerspectiveQueries
{
    private readonly ContentId _perspectiveCid;
    private readonly IPlatformPersistence _persistence;

    public PerspectiveQueries(ContentId perspectiveCid, IPlatformPersistence persistence)
    {
        _perspectiveCid = perspectiveCid;
        _persistence = persistence;
    }

    public FSharpOption<Observation<TPerception>> latest<TPerception>()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Observation<TPerception>> since<TPerception>(DateTime timestamp)
    {
        PerspectiveSequenceHead latestPerspectiveSequence;

        using (NoSynchronizationContextScope.Enter()) {
            latestPerspectiveSequence = _persistence.Get<PerspectiveSequenceHead>(_perspectiveCid).Result;
        }

        foreach (var happening in LatestObservationTreeHeadsSince(latestPerspectiveSequence, timestamp)) {
            // Only return observations of requested type TPerception. Ignore all others.
            if (happening.Node.ObservationPerceptionType != typeof(TPerception)) {
                continue;
            }

            // Load latest node in observation sequence.
            ObservationSequenceHead<TPerception> observationSequenceHead;

            using (NoSynchronizationContextScope.Enter()) {
                observationSequenceHead = _persistence.Get<ObservationSequenceHead<TPerception>>(happening.Node.ObservationSequenceHead).Result;
            }

            // If that node is an observation, return its information.
            if (observationSequenceHead is ObservationSequenceHead<TPerception>.Observation observation) {
                yield return observation.Node.Observation;
            }
        }
    }

    public IEnumerable<(FSharpOption<Observation<TPerception1>>, FSharpOption<Observation<TPerception2>>)> since<TPerception1, TPerception2>(DateTime timestamp)
    {
        throw new NotImplementedException();
    }

    private IReadOnlyList<PerspectiveSequenceHead.Happening> LatestObservationTreeHeadsSince(PerspectiveSequenceHead perspectiveSequenceHead, DateTime since)
    {
        // This is to buffer all observations that happens after 'since' until the first observation is inspected that came before 'since'.
        // This means that before anything is returned, all requested observations are loaded into memory.
        // Todo: to optimize this, the runtime should eagerly build the reversed linked list and provide an index into all nodes via a timestamp.
        // Then only a tree-node is returned and the iteration of it is on the user.
        var selectedObservations = new List<PerspectiveSequenceHead.Happening>();

        while (true) {
            if (perspectiveSequenceHead is PerspectiveSequenceHead.Happening happening) {
                if (happening.Node.At > since) {
                    selectedObservations.Add(happening);
                } else {
                    // Found first observation earlier than 'since'.
                    // Stop iteration here because the timestamps can only ever decrease into the pest.
                    break;
                }

                using (NoSynchronizationContextScope.Enter()) {
                    perspectiveSequenceHead = _persistence.Get<PerspectiveSequenceHead>(happening.Node.Previous).Result;
                }
            } else {
                // No more observations (first node of linked list)
                Debug.Assert(perspectiveSequenceHead == PerspectiveSequenceHead.Empty, "PerspectiveSequenceHead should have only two union cases: Empty | Happening");
                break;
            }
        }

        return selectedObservations;
    }
}
