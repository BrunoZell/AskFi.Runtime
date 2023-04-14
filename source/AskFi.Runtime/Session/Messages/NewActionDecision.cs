using AskFi.Persistence;

namespace AskFi.Runtime.Session.Messages;

/// <summary>
/// Message from Strategy Subsystem when a non-inaction decision has been made.
/// This message is to be picked up by the Execution Subsystem, in case live execution is configured.
/// </summary>
internal class NewActionDecision
{
    public required ContentId DecisionSequenceCid { get; init; }
    public required IReadOnlyCollection<ActionInitiation> ActionSet { get; init; }

    internal class ActionInitiation
    {
        /// <summary>
        /// The P from IObserver<P> (type of the originating observer instance)
        /// </summary>
        public required Type ActionType { get; init; }

        /// <summary>
        /// Cid to the action information. Has type of <see cref="ActionType"/>.
        /// </summary>
        public required ContentId ActionCid { get; init; }
    }
}
