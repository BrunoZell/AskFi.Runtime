namespace AskFi.Runtime.Messages

open AskFi.Runtime.Persistence

/// Message from Observer Module emitted on each new received observation
type NewObservation = {
    /// Cid to the newly produced CapturedObservation
    CapturedObservationCid: ContentId
}

/// Represents a new perspective
type NewPerspective = {
    /// Cid to the latest merged observation pool CRDT (DataModel.ObservationPool)
    ObservationPool: ContentId
}

/// Message emitted from Strategy Module when a non-inaction decision has been made
type NewDecision = {
    /// Cid to the newly created decision sequence head (DataModel.DecisionSequenceHead)
    DecisionSequenceHeadCid: ContentId
}

/// Represents the result of an action execution
type ActionExecuted = {
    /// Cid to the newly created execution sequence head (DataModel.ExecutionSequenceHead)
    ExecutionSequenceHeadCid: ContentId
}

/// Broadcasted by persistence system for other nodes to eagerly receive data referenced in other messages.
type PersistencePut = {
    Cid: ContentId
    Content: byte array
}
