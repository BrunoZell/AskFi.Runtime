using System.Diagnostics;
using AskFi.Runtime.Internal;
using AskFi.Runtime.Persistence;
using AskFi.Runtime.Platform;
using Microsoft.FSharp.Core;
using static AskFi.Runtime.DataModel;
using static AskFi.Runtime.DataModel.ContextSequenceHead;
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

    public IEnumerable<CapturedObservation<TPercept>> inTimeRange<TPercept>(DateTime from, DateTime to)
    {
        var l = List().ToList();
        return l;

        IEnumerable<CapturedObservation<TPercept>> List()
        {
            ContextSequenceHead latestContextSequenceHead;

            using (NoSynchronizationContextScope.Enter()) {
                latestContextSequenceHead = _persistence.Get<ContextSequenceHead>(_latestContextSequenceHead).Result;
            }

            foreach (var capturedObservation in ObservationsOfTypeInTimeRange<TPercept>(latestContextSequenceHead, from, to).Reverse()) {

                // Load observation from context sequence node.
                CapturedObservation<TPercept> observation;

                using (NoSynchronizationContextScope.Enter()) {
                    observation = _persistence.Get<CapturedObservation<TPercept>>(capturedObservation.Observation).Result;
                }

                yield return observation;
            }
        }
    }

    public IEnumerable<(FSharpOption<CapturedObservation<TPercept1>>, FSharpOption<CapturedObservation<TPercept2>>)> inTimeRange<TPercept1, TPercept2>(DateTime from, DateTime to)
    {
        throw new NotImplementedException();
    }

    private IReadOnlyList<CapturedObservation> ObservationsOfTypeInTimeRange<TPercept>(ContextSequenceHead contextSequenceHead, DateTime from, DateTime to)
    {
        // This is to buffer all observations that happens after 'since' until the first observation is inspected that came before 'since'.
        // This means that before anything is returned, all requested observations are loaded into memory.
        // Todo: to optimize this, the runtime should eagerly build the reversed linked list and provide an index into all nodes via a timestamp.
        // Then only a tree-node is returned and the iteration of it is on the user.
        var selectedObservations = new List<CapturedObservation>();

        while (true) {
            if (contextSequenceHead is ContextSequenceHead.Context context) {
                if (context.Node.Observation.At >= from) {
                    // Only return observations of requested type TPercept. Ignore all others.
                    if (context.Node.Observation.At < to && context.Node.Observation.PerceptType == typeof(TPercept)) {
                        selectedObservations.Add(context.Node.Observation);
                    }
                } else {
                    // Found first observation earlier than 'from'.
                    // Stop iteration here because the timestamps can only ever decrease into the pest.
                    break;
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
}
