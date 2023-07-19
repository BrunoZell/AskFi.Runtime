using System.Diagnostics;
using AskFi.Runtime.Internal;
using AskFi.Runtime.Persistence;
using AskFi.Runtime.Platform;
using Microsoft.FSharp.Core;
using static AskFi.Runtime.DataModel;
using static AskFi.Sdk;

namespace AskFi.Runtime;

public sealed class ContextQueries : IContextQueries
{
    private readonly ContentId _latestContextSequenceHead;
    private readonly IPlatformPersistence _persistence;

    public ContextQueries(ContentId latestContextSequenceHead, IPlatformPersistence persistence)
    {
        _latestContextSequenceHead = latestContextSequenceHead;
        _persistence = persistence;
    }

    public FSharpOption<CapturedObservation<TPerception>> latest<TPerception>()
    {
        ContextSequenceHead contextSequenceHead;

        using (NoSynchronizationContextScope.Enter()) {
            contextSequenceHead = _persistence.Get<ContextSequenceHead>(_latestContextSequenceHead).Result;
        }

        while (true) {
            if (contextSequenceHead is not ContextSequenceHead.Context context) {
                throw new InvalidOperationException($"No observations of type {typeof(TPerception).FullName} the context sequence. Reached the identity node of the context sequence. No more observations to inspect.");
            }

            if (context.Node.Observation.PerceptType == typeof(TPerception)) {
                // Percept type fits. Load and return.
                using (NoSynchronizationContextScope.Enter()) {
                    return _persistence.Get<CapturedObservation<TPerception>>(context.Node.Observation.Observation).Result;
                }
            } else {
                // Look for immediate predecessor.
                using (NoSynchronizationContextScope.Enter()) {
                    contextSequenceHead = _persistence.Get<ContextSequenceHead>(context.Node.Previous).Result;
                }
            }
        }
    }

    public IEnumerable<CapturedObservation<TPerception>> since<TPerception>(DateTime timestamp)
    {
        ContextSequenceHead latestContextSequenceHead;

        using (NoSynchronizationContextScope.Enter()) {
            latestContextSequenceHead = _persistence.Get<ContextSequenceHead>(_latestContextSequenceHead).Result;
        }

        foreach (var context in LatestContxtSequenceFromSinceToLatest(latestContextSequenceHead, timestamp)) {
            // Only return observations of requested type TPerception. Ignore all others.
            if (context.Node.Observation.PerceptType != typeof(TPerception)) {
                continue;
            }

            // Load observation from context sequence node.
            CapturedObservation<TPerception> observation;

            using (NoSynchronizationContextScope.Enter()) {
                observation = _persistence.Get<CapturedObservation<TPerception>>(context.Node.Previous).Result;
            }

            yield return observation;
        }
    }

    public IEnumerable<(FSharpOption<CapturedObservation<Perception1>>, FSharpOption<CapturedObservation<Perception2>>)> since<Perception1, Perception2>(DateTime timestamp)
    {
        throw new NotImplementedException();
    }

    private IReadOnlyList<ContextSequenceHead.Context> LatestContxtSequenceFromSinceToLatest(ContextSequenceHead contextSequenceHead, DateTime since)
    {
        // This is to buffer all observations that happens after 'since' until the first observation is inspected that came before 'since'.
        // This means that before anything is returned, all requested observations are loaded into memory.
        // Todo: to optimize this, the runtime should eagerly build the reversed linked list and provide an index into all nodes via a timestamp.
        // Then only a tree-node is returned and the iteration of it is on the user.
        var selectedObservations = new List<ContextSequenceHead.Context>();

        while (true) {
            if (contextSequenceHead is ContextSequenceHead.Context context) {
                if (context.Node.Observation.At > since) {
                    selectedObservations.Add(context);
                } else {
                    // Found first observation earlier than 'since'.
                    // Stop iteration here because the timestamps can only ever decrease into the pest.
                    break;
                }

                using (NoSynchronizationContextScope.Enter()) {
                    contextSequenceHead = _persistence.Get<ContextSequenceHead>(context.Node.Previous).Result;
                }
            } else {
                // No more observations (first node of linked list)
                Debug.Assert(contextSequenceHead is ContextSequenceHead.Identity, "PerspectiveSequenceHead should have only two union cases: Identity | Context");
                break;
            }
        }

        return selectedObservations;
    }
}
