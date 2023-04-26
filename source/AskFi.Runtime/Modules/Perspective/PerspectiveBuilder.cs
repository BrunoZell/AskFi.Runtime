using System.Collections.Immutable;
using AskFi.Runtime.Persistence;
using AskFi.Runtime.Platform;
using static AskFi.Runtime.DataModel;

namespace AskFi.Runtime.Modules.Perspective;

internal class PerspectiveBuilder
{
    private PerspectiveBuilder(
        ImmutableHashSet<ContentId> observationSet,
        ImmutableSortedDictionary<DateTime, ContentId> timestampMap)
    {
        _allIncludedLinkedObservations = observationSet;
        _absouteTimestampMap = timestampMap;
    }

    private PerspectiveBuilder()
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

    public async ValueTask<PerspectiveBuilder> WithObservation<TPerception>(ContentId linkedObservationCid, IPlatformPersistence persistence)
    {
        var newObservationSet = _allIncludedLinkedObservations.Add(linkedObservationCid);

        if (newObservationSet == _allIncludedLinkedObservations) {
            // Observation already included
            return this;
        }

        var linkedObservation = await persistence.Get<LinkedObservation>(linkedObservationCid);
        var capturedObservation = await persistence.Get<CapturedObservation<TPerception>>(linkedObservation.Observation);

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
            return new PerspectiveBuilder(newObservationSet, updatedTimestampMap);
        } else {
            // New discrete timestamp. Build on perspective before
            var newObservationPerspective = PerspectiveSequenceHead.NewHappening(new(
                previous: latestPerspectiveSequenceCidOnSameTimestamp,
                linkedObservation: linkedObservationCid));

            var newObservationPerspectiveCid = await persistence.Put(newObservationPerspective);

            var updatedTimestampMap = trimmedTimestampMap.Add(capturedObservation.At, newObservationPerspectiveCid);
            return new PerspectiveBuilder(newObservationSet, updatedTimestampMap);
        }
    }
}
