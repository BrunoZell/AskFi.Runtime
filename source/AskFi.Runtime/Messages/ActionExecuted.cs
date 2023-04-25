using AskFi.Runtime.Persistence;

namespace AskFi.Runtime.Messages;

public class ActionExecuted
{
    /// <summary>
    /// The P from IObserver<P> (type of the originating observer instance)
    /// </summary>
    public required Type ActionType { get; init; }

    /// <summary>
    /// Cid to the action information. Has type of <see cref="ActionType"/>.
    /// </summary>
    public required ContentId ActionCid { get; init; }

    /// <summary>
    /// Data emitted by the IBroker action execution.
    /// Could include an execution id, transaction, or validity proofs.
    /// </summary>
    public byte[]? ExecutionTrace { get; init; }

    public string? UserException { get; init; }
}
