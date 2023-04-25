namespace AskFi.Runtime.Messages

open System
open AskFi.Runtime.Persistence

/// Message from Observer Module emitted on each new received observation
type NewObservation = {
    /// The 'Percept from IObserver<'Percept> to full parse the DataModel.LinkedObservation<'Percept>
    PerceptionType: Type

    /// Cid to the newly produced LinkedObservation<'Percept>
    LinkedObservationCid: ContentId
}

/// Represents a new perspective
type NewPerspective = {
    /// Cid to the newly created perspective sequence node (DataModel.PerspectiveSequenceNode)
    PerspectiveSequenceNodeCid: ContentId

    /// How many nodes before the one specified above have been
    /// rewritten compared to the previously emitted new perspective
    /// of the perspective module instance.
    /// A rewrite depth of zero means only the latest node has been
    /// added an no historic rewrites occured.
    RewriteDepth: uint
}

/// Message emitted from Strategy Module when a non-inaction decision has been made
type NewDecision = {
    /// Cid to the newly created decision sequence node (DataModel.DecisionSequenceNode)
    DecisionSequenceNodeCid: ContentId
}

/// Represents the result of an action execution
type ActionExecuted = {
    /// Cid to the newly created execution sequence node (DataModel.ExecutionSequenceNode)
    ExecutionSequenceNodeCid: ContentId
}
