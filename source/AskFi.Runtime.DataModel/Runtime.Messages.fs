namespace AskFi.Runtime.Messages

open System
open AskFi.Runtime.Persistence

/// Message from Observer Module emitted on each new received observation
type NewObservation = {
    /// The 'Percept from IObserver<'Percept> to full parse the DataModel.LinkedObservation<'Percept>
    PerceptType: Type

    /// Cid to the newly produced LinkedObservation<'Percept>
    LinkedObservationCid: ContentId
}

/// Represents a new perspective
type NewPerspective = {
    /// Cid to the newly created perspective sequence head (DataModel.PerspectiveSequenceHead)
    PerspectiveSequenceHeadCid: ContentId

    /// How many nodes before the one specified above have been
    /// rewritten compared to the previously emitted new perspective
    /// of the perspective module instance.
    /// A rewrite depth of zero means only the latest node has been
    /// added an no historic rewrites occured.
    RewriteDepth: uint
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
