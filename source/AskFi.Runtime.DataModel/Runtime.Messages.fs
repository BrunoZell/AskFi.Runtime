module AskFi.Runtime.Messages

open System
open AskFi.Runtime.Persistence

/// Message from Observer Module emitted on each new received observation
type NewObservation = {
    /// The 'Percept from IObserver<'Percept> to full parse the DataModel.LinkedObservation
    PerceptionType: Type
    /// Cid to the newly produced LinkedObservation
    LinkedObservationCid: ContentId
}

/// Represents a new perspective
type NewPerspective = {
    /// Cid to the latest DataModel.PerspectiveSequenceHead
    PerspectiveSequenceHeadCid: ContentId
}

/// Message emitted from Strategy Module when a non-inaction decision has been made
type NewDecision = {
    /// Cid to the decision sequence
    DecisionSequenceCid: ContentId
    /// Collection of action initiations
    ActionSet: ActionInitiation list
}
and ActionInitiation = {
    /// The 'Action from IBroker<'Action> (type of the originating observer instance)
    ActionType: Type
    /// Cid to the action information. Has type of ActionType.
    ActionCid: ContentId
}

/// Represents the result of an action execution
type ActionExecuted = {
    /// The 'Action from IObserver<'Action> (type of the originating observer instance)
    ActionType: Type
    /// Cid to the action information. Has type of ActionType.
    ActionCid: ContentId
    /// Data emitted by the IBroker action execution. Could include an execution id, transaction, or validity proofs.
    ExecutionTrace: byte[] option
    /// An exception message, if any, encountered during user code execution
    UserException: string option
}
