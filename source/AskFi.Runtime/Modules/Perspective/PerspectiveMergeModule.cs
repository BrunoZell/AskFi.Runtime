using System.Threading.Channels;
using AskFi.Runtime.Messages;
using AskFi.Runtime.Platform;

namespace AskFi.Runtime.Modules.Perspective;
internal class PerspectiveMergeModule : IPerspectiveMergeModule
{
    private readonly Channel<NewPerspective> _input;
    private readonly Channel<NewPerspective> _output;
    private readonly IPlatformPersistence _persistence;

    ChannelWriter<NewPerspective> IPerspectiveMergeModule.Input => _input.Writer;
    ChannelReader<NewPerspective> IPerspectiveMergeModule.Output => _output.Reader;

    public PerspectiveMergeModule(IPlatformPersistence persistence)
    {
        _input = Channel.CreateUnbounded<NewPerspective>();
        _output = Channel.CreateUnbounded<NewPerspective>();
        _persistence = persistence;
    }

    public async Task Run(CancellationToken cancellationToken)
    {
        await foreach (var newPerspective in _input.Reader.ReadAllAsync(cancellationToken)) {
            // Todo: Merge perspectives
            // Todo: Publish state
            var newMergedPerspective = new NewPerspective(
                perspectiveSequenceNodeCid: default,
                rewriteDepth: 0);

            await _output.Writer.WriteAsync(newMergedPerspective);
        }
    }
}
