using System.Collections.Immutable;
using AskFi.Runtime.Persistence;
using AskFi.Runtime.Platform;
using static AskFi.Runtime.DataModel;

namespace AskFi.Runtime.Modules.Perspective;

internal class ObservationPoolMerger
{
    private ObservationPoolMerger(
        ImmutableHashSet<ContentId> observationSet,
        ImmutableSortedDictionary<DateTime, ContentId> timestampMap)
    {
        _allIncludedLinkedObservations = observationSet;
        _absouteTimestampMap = timestampMap;
    }

    private ObservationPoolMerger()
    {
        _allIncludedLinkedObservations = ImmutableHashSet<ContentId>.Empty;
        _absouteTimestampMap = ImmutableSortedDictionary<DateTime, ContentId>.Empty;
    }

    /// <summary>
    /// Set of all <see cref="ContentId"/> of <see cref="LinkedObservation"/> included in this builders perspective
    /// </summary>
    private readonly ImmutableHashSet<ContentId> _allIncludedLinkedObservations;

    /// <summary>
    /// Maps every recorded discrete timestamp to the <see cref="ContentId"/> of the <see cref="PerspectiveSequenceNode"/> with
    /// the latest observation recorded at that timestamp.
    /// </summary>
    private readonly ImmutableSortedDictionary<DateTime, ContentId> _absouteTimestampMap;

    public static async ValueTask<ObservationPool> Add(ObservationPool a, ObservationPool b, IPlatformPersistence persistence)
    {
        // 1: Find first common ancestor (which is the point where all previous observations are sequenced in exactly the same way)

        async ValueTask<ContentId> FirstCommonAncestor()
        {
            // 1. Peek at ancestor perspective of a' or b' with the latest latest timestamp
            // 2. If that perspectives content id is in the set, it's the first common ancestor
            // 3. If not, add it to the set and continue crawling throug ancestors
        }

        // 2: Re-apply remaining known observations by smallest timestamp first.

        // Remove all perspective cids from the set from 1. which are included in the first common ancestor.
        // Then sort all observations referenced by the remaining perspective cids by their trusted timestamp.
        // Pick best & build perspective until no observations are left

        // 3: Memoize all perspective-cids that existed in either a or b but not in the result anymore and add them to 'droppedPerspectives'.

        var newObservationSet = _allIncludedLinkedObservations.Add(linkedObservationCid);

        if (newObservationSet == _allIncludedLinkedObservations) {
            // Observation already included
            return this;
        }

        var linkedObservation = await persistence.Get<LinkedObservation>(linkedObservationCid);
        var capturedObservation = await persistence.Get<CapturedObservation<TPercept>>(linkedObservation.Observation);

        var invalidatedPerspectives = _absouteTimestampMap
            .Where(kvp => kvp.Key > capturedObservation.At);

        var trimmedTimestampMap = _absouteTimestampMap
            .RemoveRange(invalidatedPerspectives.Select(kvp => kvp.Key));

        // Todo: Rebuild all Perspective Sequence Nodes that changed in history. For now they are just removed.
        //foreach (var (timestamp, invalidatedPerspectiveSequenceHeadCid) in invalidatedPerspectives) {
        //    var invalidatedPerspectiveHead = await persistence.Get<PerspectiveSequenceHead>(invalidatedPerspectiveSequenceHeadCid);
        //    var invalidatedPerspectiveNode = (invalidatedPerspectiveHead as PerspectiveSequenceHead.Happening).Node;
        //    var l = invalidatedPerspectiveNode.LinkedObservation;
        //}

        if (_absouteTimestampMap.TryGetValue(capturedObservation.At, out var latestPerspectiveSequenceCidOnSameTimestamp)) {
            // There already is an observation on that exact same discrete timestamp.
            var newObservationPerspective = PerspectiveSequenceHead.NewHappening(new(
                previous: latestPerspectiveSequenceCidOnSameTimestamp,
                linkedObservation: linkedObservationCid));

            var newObservationPerspectiveCid = await persistence.Put(newObservationPerspective);

            var updatedTimestampMap = trimmedTimestampMap.Add(capturedObservation.At, newObservationPerspectiveCid);
            return new ObservationPoolMerger(newObservationSet, updatedTimestampMap);
        } else {
            // New discrete timestamp. Build on perspective before
            var newObservationPerspective = PerspectiveSequenceHead.NewHappening(new(
                previous: latestPerspectiveSequenceCidOnSameTimestamp,
                linkedObservation: linkedObservationCid));

            var newObservationPerspectiveCid = await persistence.Put(newObservationPerspective);

            var updatedTimestampMap = trimmedTimestampMap.Add(capturedObservation.At, newObservationPerspectiveCid);
            return new ObservationPoolMerger(newObservationSet, updatedTimestampMap);
        }
    }
}
