using AskFi.Runtime.Messages;
using AskFi.Runtime.Modules.Observation;
using AskFi.Runtime.Modules.Output;
using AskFi.Runtime.Modules.Perspective;
using AskFi.Runtime.Platform;

namespace AskFi.Runtime;

public class Scraper
{
    private readonly ObserverModule _observerModule;
    private readonly PerspectiveModule _perspectiveModule;
    private readonly EmitOutput<NewObservationPool> _output;

    private Scraper(
        ObserverModule observerModule,
        PerspectiveModule perspectiveModule,
        EmitOutput<NewObservationPool> output)
    {
        _observerModule = observerModule;
        _perspectiveModule = perspectiveModule;
        _output = output;
    }

    /// <param name="observers">TValue = <see cref="Sdk.IObserver{Percept}"/> (where Percept = .Key)</param>
    public static Scraper Build(
        IReadOnlyDictionary<Type, object> observers,
        IPlatformPersistence persistence,
        IPlatformMessaging messaging)
    {
        var observation = new ObserverModule(observers, persistence);
        var perspectiveModule = new PerspectiveMergeModule(persistence, observation.Output);
        var output = new EmitOutput<NewObservationPool>(messaging, perspectiveModule.Output);

        return new(observation, perspectiveModule, output);
    }

    public async Task Run(CancellationToken shutdown)
    {
        var observerTask = _observerModule.Run(shutdown);
        var perspectiveTask = _perspectiveModule.Run(shutdown);
        var outputTask = _output.Run();

        await Task.WhenAll(observerTask, perspectiveTask, outputTask);
    }
}
