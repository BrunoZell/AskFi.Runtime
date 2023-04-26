using AskFi.Runtime.Messages;
using AskFi.Runtime.Modules.Input;
using AskFi.Runtime.Modules.Output;
using AskFi.Runtime.Modules.Strategy;
using AskFi.Runtime.Platform;
using static AskFi.Sdk;

namespace AskFi.Runtime;

public class Evaluator
{
    private readonly StreamInput<NewPerspective> _input;
    private readonly StrategyModule _strategyModule;
    private readonly EmitOutput<NewDecision> _output;

    private Evaluator(
        StreamInput<NewPerspective> input,
        StrategyModule strategyModule,
        EmitOutput<NewDecision> output)
    {
        _input = input;
        _strategyModule = strategyModule;
        _output = output;
    }

    public static Evaluator Build(
        Func<Reflection, Perspective, Decision> strategy,
        IPlatformPersistence persistence,
        IPlatformMessaging messaging)
    {
        var input = new StreamInput<NewPerspective>(messaging);
        var strategyModule = new StrategyModule(strategy, persistence, input.Output);
        var output = new EmitOutput<NewDecision>(messaging, strategyModule.Output);

        return new(input, strategyModule, output);
    }

    public async Task Run(CancellationToken shutdown)
    {
        var inputTask = _input.Run(shutdown);
        var strategyTask = _strategyModule.Run(shutdown);
        var outputTask = _output.Run();

        await Task.WhenAll(inputTask, strategyTask, outputTask);
    }
}
