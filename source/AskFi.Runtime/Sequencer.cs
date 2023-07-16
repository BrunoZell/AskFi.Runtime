using AskFi.Runtime.Messages;
using AskFi.Runtime.Modules.Input;
using AskFi.Runtime.Modules.Output;
using AskFi.Runtime.Modules.Perspective;
using AskFi.Runtime.Platform;

namespace AskFi.Runtime;

public class Sequencer
{
    private readonly StreamInput<NewObservationPool> _input;
    private readonly ObservationPoolDeduplicatorModule _perspectiveMergeModule;
    private readonly EmitOutput<NewObservationPool> _output;

    private Sequencer(
        StreamInput<NewObservationPool> input,
        ObservationPoolDeduplicatorModule perspectiveMergeModule,
        EmitOutput<NewObservationPool> output)
    {
        _input = input;
        _perspectiveMergeModule = perspectiveMergeModule;
        _output = output;
    }

    public static Sequencer Build(
        IPlatformPersistence persistence,
        IPlatformMessaging messaging)
    {
        var input = new StreamInput<NewObservationPool>(messaging);
        var perspectiveMergeModule = new ObservationPoolModule(persistence, input.Output);
        var output = new EmitOutput<NewObservationPool>(messaging, perspectiveMergeModule.Output);

        return new(input, perspectiveMergeModule, output);
    }

    public async Task Run(CancellationToken shutdown)
    {
        var inputTask = _input.Run(shutdown);
        var perspectiveTask = _perspectiveMergeModule.Run(shutdown);
        var outputTask = _output.Run();

        await Task.WhenAll(inputTask, perspectiveTask, outputTask);
    }
}
