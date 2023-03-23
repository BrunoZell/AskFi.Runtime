using System.Threading.Channels;
using static AskFi.Runtime.DataModel;

namespace AskFi.Runtime.Internal;

/// <summary>
/// Wraps an <see cref="Sdk.IObserver{Perception}"/> and does basic bookkeeping around the incomding
/// observations from that observer. To then forward it to the passed observation sink, which is this
/// sessions <see cref="WorldSequencer"/>.
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
        ChannelWriter<OnNewObservation> observationSink,
        CancellationToken sessionShutdown)
    {
        var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(sessionShutdown);
        var backgroundTask = PullObservations(observer, observationSink, linkedCancellation.Token);
        return new ObserverSequencer(backgroundTask, linkedCancellation);
    }

    /// <summary>
    /// This background tasks interates <see cref="Sdk.IObserver{T}.Observations"/> (once per observer instance)
    /// and sequences it into an <see cref="ObservationStreamHead{Perception}"/>.
    /// The new latest <see cref="ObservationStreamHead{Perception}"/> is then passed to the <see cref="WorldSequencer"/> for session-wide sequencing.
    /// </summary>
    private static async Task PullObservations<TPerception>(
        Sdk.IObserver<TPerception> observer,
        ChannelWriter<OnNewObservation> observationSink,
        CancellationToken cancellationToken)
    {
        await Task.Yield();

        try {
            var streamHead = ObservationStreamHead<TPerception>.Beginning;
            await foreach (var observation in observer.Observations.WithCancellation(cancellationToken)) {
                var sequencedObservation = new Observation<TPerception>(observation.Perceptions, previous: streamHead);
                streamHead = ObservationStreamHead<TPerception>.NewObservation(sequencedObservation);

                // Todo: Send to persistence subsystem to serialize, put & pin in IPFS cluster. + insertig according metadata in etcd
                // Todo: Maybe also eagerly build an index of observation session correlations: ObservationSession -> Observation<_> list

                await observationSink.WriteAsync(new OnNewObservation() {
                    PerceptionType = typeof(TPerception),
                    ObservationStreamHead = streamHead,
                    Session = new ObservationSessionKey() {
                        ObserverInstance = observer,
                        ObserverProvidedSessionKey = observation.Session,
                    },
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
        await _backgroundTask; // To throw and observe possible excpetions.
    }
}
