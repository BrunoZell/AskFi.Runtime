using System.Threading.Channels;
using AskFi.Runtime.Messages;
using AskFi.Runtime.Persistence;
using AskFi.Runtime.Platform;
using static AskFi.Runtime.DataModel;

namespace AskFi.Runtime.Modules.Observation;

/// <summary>
/// A single instance to pipe observations from all <see cref="ObserverInstance"/> (<see cref="Sdk.IObserver{Perception}"/>)
/// through in an async way. This is to sequence it in a first-come-first-server way. After new observations
/// are written to the <see cref="ObserverGroup"/>, their observation order is set, and all conclusions derived are
/// deterministic (and thus reproducible) thereafter.
/// </summary>
internal sealed class ObserverGroup : IAsyncDisposable
{
    private readonly IReadOnlyCollection<ObserverInstance> _observers;
    private readonly Channel<NewInternalObservation> _incomingObservations;
    private readonly ChannelWriter<NewObservation> _output;
    private readonly IPlatformPersistence _persistence;
    private readonly CancellationTokenSource _cancellation;
    private readonly Task _backgroundTask;

    private ObserverGroup(
        IReadOnlyCollection<ObserverInstance> observers,
        Channel<NewInternalObservation> incomingObservations,
        IPlatformPersistence persistence,
        ChannelWriter<NewObservation> output,
        CancellationTokenSource cancellation)
    {
        _observers = observers;
        _incomingObservations = incomingObservations;
        _output = output;
        _persistence = persistence;
        _cancellation = cancellation;
        _backgroundTask = LinkObservations();
    }

    public static ObserverGroup StartNew(
        /*IObserver<'Percept> (where Percept = .Key)*/ IReadOnlyDictionary<Type, object> observers,
        IPlatformPersistence persistence,
        ChannelWriter<NewObservation> output,
        CancellationToken cancellationToken)
    {
        var distinctObserverInstances = observers.Values
            .Distinct(ReferenceEqualityComparer.Instance)
            .Count();

        if (distinctObserverInstances != observers.Count) {
            throw new ArgumentException("Each distinct IObserver-instance can only be operated once by the Observer Module. The passed list containes double entries.");
        }

        var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var incomingObservations = Channel.CreateUnbounded<NewInternalObservation>();
        var observerInstances = observers
            .Select(o => ObserverInstance.StartNew(o.Key, o.Value, incomingObservations, persistence, linkedCancellation.Token))
            .ToArray();

        return new ObserverGroup(
            observerInstances,
            incomingObservations,
            persistence,
            output,
            linkedCancellation);
    }

    /// <summary>
    /// Long-running background task that reads all pooled new observations and builds <see cref="LinkedObservation"/> for each one.
    /// This introduces a relative ordering in time between observations of the same ObserverGroup.
    /// </summary>
    private async Task LinkObservations()
    {
        var latestObservations = new Dictionary<object, ContentId>(ReferenceEqualityComparer.Instance);

        // Sequentially receives all observations from IObserver-instances in this group as they happen.
        await foreach (var newObservation in _incomingObservations.Reader.ReadAllAsync(_cancellation.Token)) {
            latestObservations[newObservation.ObserverInstance] = newObservation.CapturedObservationCid;

            var relativeTimeLinks = latestObservations
                .Where(kvp => !ReferenceEquals(kvp.Key, newObservation.ObserverInstance))
                .Select(o => new RelativeTimeLink(o.Value))
                .ToArray();

            var linkedObservation = new LinkedObservation(newObservation.CapturedObservationCid, relativeTimeLinks);

            // Perf: Generate CID localy and upload in the background
            var linkedObservationCid = await _persistence.Put(linkedObservation);

            await _output.WriteAsync(new NewObservation(
                newObservation.PerceptionType,
                linkedObservationCid));
        }
    }

    public async ValueTask DisposeAsync()
    {
        _cancellation.Cancel();

        // To throw and observe possible exceptions.
        await Task.WhenAll(
            _observers.Select(o => o.DisposeAsync().AsTask()).Append(_backgroundTask).ToArray()
        );

        _cancellation.Dispose();
    }
}
