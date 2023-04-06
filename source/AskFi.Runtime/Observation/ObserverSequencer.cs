using System.Threading.Channels;
using AskFi.Runtime.Observation.Objects;
using AskFi.Runtime.Persistence;
using static AskFi.Runtime.DataModel;

namespace AskFi.Runtime.Behavior;

/// <summary>
/// Wraps an <see cref="Sdk.IObserver{Perception}"/> and does basic bookkeeping around the incoming
/// observations from that observer. To then forward it to the passed observation sink, which is this
/// sessions <see cref="PerspectiveSequencer"/>.
/// </summary>
internal class ObserverSequencer : IAsyncDisposable
{
    private readonly Task _backgroundTask;
    private readonly CancellationTokenSource _cancellation;

    private ObserverSequencer(Task backgroundTask, CancellationTokenSource cancellation)
    {
        _backgroundTask = backgroundTask;
        _cancellation = cancellation;
    }

    public static ObserverSequencer StartNew<TPerception>(
        Sdk.IObserver<TPerception> observer,
        ChannelWriter<NewSequencedObservation> observationSink,
        IdeaStore ideaStore,
        StateTrace stateTrace,
        CancellationToken sessionShutdown)
    {
        var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(sessionShutdown);
        var backgroundTask = PullObservations(observer, observationSink, ideaStore, stateTrace, linkedCancellation.Token);
        return new ObserverSequencer(backgroundTask, linkedCancellation);
    }

    /// <summary>
    /// This background tasks iterates <see cref="Sdk.IObserver{T}.Observations"/> (once per observer instance)
    /// and sequences it into an <see cref="ObservationSequenceHead{Perception}"/>.
    /// The new latest <see cref="ObservationSequenceHead{Perception}"/> is then passed to the <see cref="PerspectiveSequencer"/> for session-wide sequencing.
    /// </summary>
    private static async Task PullObservations<TPerception>(
        Sdk.IObserver<TPerception> observer,
        ChannelWriter<NewSequencedObservation> observationSink,
        IdeaStore ideaStore,
        StateTrace stateTrace,
        CancellationToken cancellationToken)
    {
        await Task.Yield();

        try {
            var observationSequence = ObservationSequenceHead<TPerception>.Beginning;
            var observationSequenceCid = await ideaStore.Store(observationSequence);

            await foreach (var observation in observer.Observations.WithCancellation(cancellationToken)) {
                // Build new node onto Observation Sequence
                var observationSequenceNode = new ObservationSequenceNode<TPerception>(observation, observationSequenceCid);
                observationSequence = ObservationSequenceHead<TPerception>.NewObservation(observationSequenceNode);

                // Persist new node
                observationSequenceCid = await ideaStore.Store(observationSequence);

                // Todo: Build indices for chronological and continuous sorting

                await observationSink.WriteAsync(new NewSequencedObservation() {
                    PerceptionType = typeof(TPerception),
                    ObservationSequenceHeadCid = observationSequenceCid
                });

                stateTrace.LatestObservationSequences[typeof(TPerception)] = observationSequenceCid;
            }
#if DEBUG
        } catch (Exception ex) {
            Console.Error.WriteLine(ex.ToString());
#endif

        } finally {
            observationSink.Complete();
        }
    }

    public async ValueTask DisposeAsync()
    {
        _cancellation.Cancel();
        _cancellation.Dispose();
        await _backgroundTask; // To throw and observe possible exceptions.
    }
}
