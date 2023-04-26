using AskFi.Runtime.Messages;
using AskFi.Runtime.Modules.Perspective;
using AskFi.Runtime.Platform;

namespace AskFi.Runtime;

public class Sequencer
{
    private readonly StreamInput<NewPerspective> _input;
    private readonly PerspectiveMergeModule _perspectiveModule;
    private readonly EmitOutput<NewPerspective> _output;

    private Sequencer(
        StreamInput<NewPerspective> input,
        PerspectiveMergeModule perspectiveModule,
        EmitOutput<NewPerspective> output)
    {
        _input = input;
        _perspectiveModule = perspectiveModule;
        _output = output;
    }

    public static Sequencer Build(
        IPlatformPersistence persistence,
        IPlatformMessaging messaging)
    {
        var input = new StreamInput<NewPerspective>(messaging);
        var perspective = new PerspectiveMergeModule(persistence, input.Output);
        var output = new EmitOutput<NewPerspective>(messaging, perspective.Output);

        return new(input, perspective, output);
    }

    public async Task Run(CancellationToken shutdown)
    {
        var inputTask = _input.Run(shutdown);
        var perspectiveTask = _perspectiveModule.Run(shutdown);
        var outputTask = _output.Run();

        await Task.WhenAll(inputTask, perspectiveTask, outputTask);
    }
}
