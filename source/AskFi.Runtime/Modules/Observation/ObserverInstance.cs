using System.Diagnostics;
using System.Reflection;
using System.Threading.Channels;
using AskFi.Runtime.Platform;
using static AskFi.Runtime.DataModel;

namespace AskFi.Runtime.Modules.Observation;

/// <summary>
/// Wraps an <see cref="Sdk.IObserver{Perception}"/> and does basic bookkeeping around the incoming
/// observations from that observer. To then forward it to the passed observation sink, which is this
/// sessions <see cref="ObserverModule"/>.
/// </summary>
internal sealed class ObserverInstance : IAsyncDisposable
{
    private readonly Task _backgroundTask;
    private readonly CancellationTokenSource _cancellation;

    #region Construction
    private ObserverInstance(Task backgroundTask, CancellationTokenSource cancellation)
    {
        _backgroundTask = backgroundTask;
        _cancellation = cancellation;
    }

    public static ObserverInstance StartNew(
        /*'P*/ Type perception,
        /*IObserver<'P>*/ object observer,
        ChannelWriter<NewInternalObservation> observationSink,
        IPlatformPersistence persistence,
        CancellationToken sessionShutdown)
    {
        var observerType = typeof(Sdk.IObserver<>).MakeGenericType(perception);
        var implementsObserverType = observer.GetType()
            .IsAssignableTo(observerType);

        if (!implementsObserverType)
            throw new ArgumentException($"Parameter '{nameof(observer)} must implement 'IObserver<P>' with P = '{nameof(perception)}' (the parameter)");

        var startNew = typeof(ObserverInstance).GetMethod(
            name: nameof(ObserverInstance.StartNewInternal),
            bindingAttr: BindingFlags.Static | BindingFlags.NonPublic);

        Debug.Assert(startNew is not null, $"Function signature of {nameof(ObserverInstance.StartNew)} has changed and became incompatible with this code.");

        var startNewP = startNew.MakeGenericMethod(perception);
        var sequencer = startNewP.Invoke(obj: null, new object[] {
            observer,
            observationSink,
            persistence,
            sessionShutdown
        }) as ObserverInstance;

        Debug.Assert(sequencer is not null, $"Return type of {nameof(ObserverInstance.StartNew)} has changed and became incompatible with this code.");
        return sequencer;
    }

    private static ObserverInstance StartNewInternal<TPerception>(
        Sdk.IObserver<TPerception> observer,
        ChannelWriter<NewInternalObservation> observationSink,
        IPlatformPersistence persistence,
        CancellationToken sessionShutdown)
    {
        var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(sessionShutdown);
        var backgroundTask = PullObservations(observer, observationSink, persistence, linkedCancellation.Token);
        return new ObserverInstance(backgroundTask, linkedCancellation);
    }
    #endregion

    /// <summary>
    /// This background tasks iterates <see cref="Sdk.IObserver{T}.Observations"/> (once per observer instance)
    /// and sequences it into an <see cref="ObservationSequenceHead{Perception}"/>.
    /// The new latest <see cref="ObservationSequenceHead{Perception}"/> is then passed to the <see cref="ObserverModule"/> for session-wide sequencing.
    /// </summary>
    private static async Task PullObservations<TPerception>(
        Sdk.IObserver<TPerception> observer,
        ChannelWriter<NewInternalObservation> observationSink,
        IPlatformPersistence persistence,
        CancellationToken cancellationToken)
    {
        await Task.Yield();

        try {
            await foreach (var observation in observer.Observations.WithCancellation(cancellationToken)) {
                var timestamp = DateTime.UtcNow;

                // Capture timestamp and persist observation
                var capturedObservation = new CapturedObservation<TPerception>(timestamp, observation);

                // Perf: Generate CID localy and upload in the background
                var capturedObservationCid = await persistence.Put(capturedObservation);

                await observationSink.WriteAsync(new() {
                    CapturedObservationCid = capturedObservationCid,
                    ObserverInstance = observer,
                    PerceptionType = typeof(TPerception)
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
        await _backgroundTask; // To throw and observe possible exceptions.
        _cancellation.Dispose();
    }
}
