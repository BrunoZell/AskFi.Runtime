using AskFi.Persistence;

namespace AskFi.Runtime.Messages;

/// <summary>
/// Message emitted from Strategy Module when a non-inaction decision has been made.
/// </summary>
internal class NewDecision
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
