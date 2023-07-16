using AskFi.Runtime.Messages;
using AskFi.Runtime.Modules.Input;
using AskFi.Runtime.Modules.Output;
using AskFi.Runtime.Modules.Perspective;
using AskFi.Runtime.Platform;

namespace AskFi.Runtime;

public class ObservationGossip
{
    private readonly StreamInput<NewObservationPool> _input;
    private readonly ObservationDeduplicationModule _perspectiveMergeModule;
    private readonly EmitOutput<NewObservationPool> _output;

    private ObservationGossip(
        StreamInput<NewObservationPool> input,
        ObservationDeduplicationModule perspectiveMergeModule,
        EmitOutput<NewObservationPool> output)
    {
        _input = input;
        _perspectiveMergeModule = perspectiveMergeModule;
        _output = output;
    }

    public static ObservationGossip Build(
        IPlatformPersistence persistence,
        IPlatformMessaging messaging)
    {
        var input = new StreamInput<NewObservationPool>(messaging);
        var observationDeduplication = new ObservationDeduplicationModule(persistence, input.Output);
        var output = new EmitOutput<NewObservationPool>(messaging, observationDeduplication.Output);

        return new(input, observationDeduplication, output);
    }

    public async Task Run(CancellationToken shutdown)
    {
        var inputTask = _input.Run(shutdown);
        var perspectiveTask = _perspectiveMergeModule.Run(shutdown);
        var outputTask = _output.Run();

        await Task.WhenAll(inputTask, perspectiveTask, outputTask);
    }
}
