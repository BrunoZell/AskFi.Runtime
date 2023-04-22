namespace AskFi.Runtime.Apps;

public interface IPlatformMessaging
{
    void Emit<TMessage>(TMessage message);
    IAsyncEnumerable<TMessage> Listen<TMessage>(CancellationToken cancellationToken);
}
