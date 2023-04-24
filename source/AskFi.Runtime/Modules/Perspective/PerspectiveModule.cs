using AskFi.Runtime.Messages;
using AskFi.Runtime.Platform;

namespace AskFi.Runtime.Modules.Perspective;
internal class PerspectiveModule
{
    public async Task Run(IPlatformMessaging messaging, CancellationToken cancellationToken)
    {
        await foreach (var newObservation in messaging.Listen<NewObservation>(cancellationToken)) {
            messaging.Emit<NewPerspective>(new() {
                PerspectiveSequenceHeadCid = default
            });
        }
    }
}
