using System.Threading.Channels;
using AskFi.Runtime.Messages;
using AskFi.Runtime.Platform;

namespace AskFi.Runtime.Modules.Perspective;
internal class PerspectiveModule
{
    private readonly ChannelReader<NewObservation> _input;
    private readonly Channel<NewPerspective> _output;
    private readonly IPlatformPersistence _persistence;

    public ChannelReader<NewPerspective> Output => _output.Reader;

    public PerspectiveModule(
        IPlatformPersistence persistence,
        ChannelReader<NewObservation> input)
    {
        _input = input;
        _output = Channel.CreateUnbounded<NewPerspective>();
        _persistence = persistence;
    }

    public async Task Run(CancellationToken cancellationToken)
    {
        await foreach (var newObservation in _input.ReadAllAsync(cancellationToken)) {
            // Todo: Merge perspectives
            var newPerspective = new NewPerspective(
                perspectiveSequenceHeadCid: default,
                rewriteDepth: 0);

            await _output.Writer.WriteAsync(newPerspective);
        }
    }
}
