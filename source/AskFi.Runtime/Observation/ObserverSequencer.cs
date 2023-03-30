using System.Threading.Channels;
using AskFi.Runtime.Observation.Objects;
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
        CancellationToken sessionShutdown)
    {
        var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(sessionShutdown);
        var backgroundTask = PullObservations(observer, observationSink, linkedCancellation.Token);
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
        CancellationToken cancellationToken)
    {
        await Task.Yield();

        try {
            var streamHead = ObservationSequenceHead<TPerception>.Beginning;
            await foreach (var observation in observer.Observations.WithCancellation(cancellationToken)) {
                var observationSequenceNode = new ObservationSequenceNode<TPerception>(observation, streamHead);
                streamHead = ObservationSequenceHead<TPerception>.NewObservation(observationSequenceNode);

                // Todo: Send to persistence subsystem to serialize, put & pin in IPFS cluster. + inserting according metadata in etcd
                // Todo: Build indices for chronological and continuous sorting

                await observationSink.WriteAsync(new NewSequencedObservation() {
                    PerceptionType = typeof(TPerception),
                    ObservationSequenceHead = streamHead,
                    Observation = observation
                });
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
