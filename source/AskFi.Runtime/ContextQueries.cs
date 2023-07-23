using System.Diagnostics;
using AskFi.Runtime.Internal;
using AskFi.Runtime.Persistence;
using AskFi.Runtime.Platform;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using static AskFi.Runtime.DataModel;
using static AskFi.Runtime.Index;
using static AskFi.Sdk;

namespace AskFi.Runtime;

public sealed class ContextQueries : IContextQueries
{
    private readonly ContentId _latestContextSequenceHead;
    private readonly ContextSequenceIndex _index;
    private readonly IPlatformPersistence _persistence;

    public ContextQueries(
        ContentId latestContextSequenceHead,
        ContextSequenceIndex index,
        IPlatformPersistence persistence)
    {
        _latestContextSequenceHead = latestContextSequenceHead;
        _index = index;
        _persistence = persistence;
    }

    public FSharpOption<CapturedObservation<TPercept>> latest<TPercept>()
    {
        ContextSequenceHead contextSequenceHead;

        using (NoSynchronizationContextScope.Enter()) {
            contextSequenceHead = _persistence.Get<ContextSequenceHead>(_latestContextSequenceHead).Result;
        }

        while (true) {
            if (contextSequenceHead is not ContextSequenceHead.Context context) {
                throw new InvalidOperationException($"No observations of type {typeof(TPercept).FullName} the context sequence. Reached the identity node of the context sequence. No more observations to inspect.");
            }

            if (context.Node.Observation.PerceptType == typeof(TPercept)) {
                // Percept type fits. Load and return.
                using (NoSynchronizationContextScope.Enter()) {
                    return _persistence.Get<CapturedObservation<TPercept>>(context.Node.Observation.Observation).Result;
                }
            } else {
                // Look for immediate predecessor.
                using (NoSynchronizationContextScope.Enter()) {
                    contextSequenceHead = _persistence.Get<ContextSequenceHead>(context.Node.Previous).Result;
                }
            }
        }
    }

    public async IEnumerable<CapturedObservation<TPercept>> inTimeRange<TPercept>(DateTime from, DateTime to)
    {
        // Sort timestamps
        // Todo: Make index.Timestamp a sorted set at rest, so that not each query needs to sort all timestamps again.
        var timestampIndex = await _persistence.Get<FSharpMap<DateTime, ulong>>(_index.Timestamps);
        var sortedSet = timestampIndex.Keys.ToList();
        sortedSet.Sort();

        // Look up 'from' in index.Timestamp. Take the earliest actual timestamp >= from
        var fromOrdinal = timestampIndex[sortedSet[BinarySearchClosest(sortedSet, from)]];

        // Look up 'to' in index.Timestamp. Take the latest actual timestamp < to
        var toOrdinal = timestampIndex[sortedSet[BinarySearchClosest(sortedSet, to)]];

        // Map<uint64, ContentId<ContextSequenceHead.Context>>
        var pointerIndex = await _persistence.Get<FSharpMap<ulong, ContentId>>(_index.Pointer);
        var latestContextHead = pointerIndex[fromOrdinal];
        var observationCount = fromOrdinal - toOrdinal;

        ContextSequenceHead contextSequenceHead;

        using (NoSynchronizationContextScope.Enter()) {
            contextSequenceHead = _persistence.Get<ContextSequenceHead>(latestContextHead).Result;
        }

        foreach (var capturedObservation in ObservationsOfTypeFromLatestToFrom<TPercept>(contextSequenceHead, from).Reverse()) {
            // Load observation from context sequence node.
            CapturedObservation<TPercept> observation;

            using (NoSynchronizationContextScope.Enter()) {
                observation = _persistence.Get<CapturedObservation<TPercept>>(capturedObservation.Observation).Result;
            }

            yield return observation;
        }
    }

    public IEnumerable<(FSharpOption<CapturedObservation<TPercept1>>, FSharpOption<CapturedObservation<TPercept2>>)> inTimeRange<TPercept1, TPercept2>(DateTime from, DateTime to)
    {
        throw new NotImplementedException();
    }

    private IReadOnlyList<CapturedObservation> ObservationsOfTypeFromLatestToFrom<TPercept>(ContextSequenceHead contextSequenceHead, DateTime from)
    {
        // This is to buffer all observations that happens after 'since' until the first observation is inspected that came before 'since'.
        // This means that before anything is returned, all requested observations are loaded into memory.
        // Todo: to optimize this, the runtime should eagerly build the reversed linked list and provide an index into all nodes via a timestamp.
        // Then only a tree-node is returned and the iteration of it is on the user.
        var selectedObservations = new List<CapturedObservation>();

        while (true) {
            if (contextSequenceHead is ContextSequenceHead.Context context) {
                if (context.Node.Observation.At < from) {
                    // Found first observation earlier than 'from'.
                    // Stop iteration here because the timestamps can only ever decrease into the pest.
                    break;
                } else {
                    // Only return observations of requested type TPercept. Ignore all others.
                    if (context.Node.Observation.PerceptType == typeof(TPercept)) {
                        selectedObservations.Add(context.Node.Observation);
                    }
                }

                using (NoSynchronizationContextScope.Enter()) {
                    contextSequenceHead = _persistence.Get<ContextSequenceHead>(context.Node.Previous).Result;
                }
            } else {
                // No more observations (first node of linked list)
                Debug.Assert(contextSequenceHead is ContextSequenceHead.Identity, $"{nameof(ContextSequenceHead)} should have only two union cases: Identity | Context");
                break;
            }
        }

        return selectedObservations;
    }

    private static int BinarySearchClosest(List<DateTime> a, DateTime item)
    {
        int lowerBound = 0;
        int upperBound = a.Count - 1;
        int mid = 0;

        do {
            mid = lowerBound + ((upperBound - lowerBound) / 2);

            if (item > a[mid]) {
                lowerBound = mid + 1;
            } else {
                upperBound = mid - 1;
            }

            if (a[mid] == item) {
                return mid;
            }

        } while (lowerBound <= upperBound);

        return mid;
    }
}
