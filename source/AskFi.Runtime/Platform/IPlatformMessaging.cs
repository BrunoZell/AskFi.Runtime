using System.Runtime.CompilerServices;

namespace AskFi.Runtime.Platform;

public interface IPlatformMessaging
{
    void Emit<TMessage>(TMessage message);
    IAsyncEnumerable<TMessage> Listen<TMessage>([EnumeratorCancellation] CancellationToken cancellationToken);
}
