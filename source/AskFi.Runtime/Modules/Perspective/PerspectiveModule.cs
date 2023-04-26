using System.Threading.Channels;
using AskFi.Runtime.Messages;
using AskFi.Runtime.Platform;

namespace AskFi.Runtime.Modules.Perspective;
internal class PerspectiveModule : IPerspectiveModule
{
    private readonly Channel<NewObservation> _input;
    private readonly Channel<NewPerspective> _output;
    private readonly IPlatformPersistence _persistence;

    ChannelWriter<NewObservation> IPerspectiveModule.Input => _input.Writer;
    ChannelReader<NewPerspective> IPerspectiveModule.Output => _output.Reader;

    public PerspectiveModule(IPlatformPersistence persistence)
    {
        _input = Channel.CreateUnbounded<NewObservation>();
        _output = Channel.CreateUnbounded<NewPerspective>();
        _persistence = persistence;
    }

    public async Task Run(CancellationToken cancellationToken)
    {
        await foreach (var newObservation in _input.Reader.ReadAllAsync(cancellationToken)) {
            // Todo: Merge perspectives
            // Todo: Publish state
            var newPerspective = new NewPerspective(
                perspectiveSequenceNodeCid: default,
                rewriteDepth: 0);

            await _output.Writer.WriteAsync(newPerspective);
        }
    }
}
