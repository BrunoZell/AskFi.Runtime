using System.Threading.Channels;
using AskFi.Runtime.Messages;
using AskFi.Runtime.Platform;

namespace AskFi.Runtime.Modules.Perspective;
internal class PerspectiveMergeModule
{
    private readonly ChannelReader<NewPerspective> _input;
    private readonly Channel<NewPerspective> _output;
    private readonly IPlatformPersistence _persistence;

    public ChannelReader<NewPerspective> Output => _output.Reader;

    public PerspectiveMergeModule(
        IPlatformPersistence persistence,
        ChannelReader<NewPerspective> input)
    {
        _input = input;
        _output = Channel.CreateUnbounded<NewPerspective>();
        _persistence = persistence;
    }

    public async Task Run(CancellationToken cancellationToken)
    {
        await foreach (var newPerspective in _input.ReadAllAsync(cancellationToken)) {
            // Todo: Merge perspectives
            var newMergedPerspective = new NewPerspective(
                perspectiveSequenceNodeCid: default,
                rewriteDepth: 0);

            await _output.Writer.WriteAsync(newMergedPerspective);
        }
    }
}
