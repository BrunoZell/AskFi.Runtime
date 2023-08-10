using System.Diagnostics;
using AskFi.Runtime.Internal;
using AskFi.Runtime.Persistence;
using AskFi.Runtime.Platform;
using static AskFi.Runtime.DataModel;

namespace AskFi.Runtime.Modules.Context;

internal sealed class ContextIndex
{
    private readonly SortedDictionary<DateTime, ContentId> _timestampNodeMap;

    private ContextIndex(SortedDictionary<DateTime, ContentId> timestampNodeMap)
    {
        _timestampNodeMap = timestampNodeMap;
    }

    public IEnumerable<ContentId> ForwardWalk(DateTime from, DateTime to)
    {
        foreach (var (timestamp, node) in _timestampNodeMap) {
            if (timestamp >= from) {
                yield return node;
            } else if (timestamp >= to) {
                yield break;
            }
        }
    }

    public static ContextIndex Build(ContentId contextSequenceHeadCid, IPlatformPersistence persistence)
    {
        var currentContextSequenceHeadCid = contextSequenceHeadCid;
        var timestamps = new SortedDictionary<DateTime, ContentId>();
        var contextSequenceHead = persistence.Get<ContextSequenceHead>(currentContextSequenceHeadCid).Result;

        while (true) {
            if (contextSequenceHead is ContextSequenceHead.Context context) {

                timestamps.Add(context.Node.Observation.At, currentContextSequenceHeadCid);

                using (NoSynchronizationContextScope.Enter()) {
                    currentContextSequenceHeadCid = context.Node.Previous;
                    contextSequenceHead = persistence.Get<ContextSequenceHead>(currentContextSequenceHeadCid).Result;
                }
            } else {
                // No more observations (first node of linked list)
                Debug.Assert(contextSequenceHead is ContextSequenceHead.Identity, $"{nameof(ContextSequenceHead)} should have only two union cases: Identity | Context");
                break;
            }
        }

        return new ContextIndex(timestamps);
    }
}
