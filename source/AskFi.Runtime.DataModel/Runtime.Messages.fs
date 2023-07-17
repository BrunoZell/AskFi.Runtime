namespace AskFi.Runtime.Messages

open AskFi.Runtime.Persistence

/// Message from Observer Module emitted on each new received observation,
/// hopefully appended to its previous version of the single observation sequence it produces.
type NewObservation = {
    /// Cid to the original ObservationSequenceHead.Identity of the newly produced observation sequence head.
    Identity: ContentId

    /// Cid to the newly produced ObservationSequenceHead.Observation
    Head: ContentId
}

/// Nodes send this message to gossip the observation pool.
/// Observation pool merger send them out when they received a NewObservation message.
type NewObservationPool = {
    /// Cid to the latest merged observation pool CRDT (DataModel.ObservationPool)
    ObservationPool: ContentId
}

/// Message emitted from Strategy Module when a non-inaction decision has been made
type NewDecision = {
    /// Cid to the newly created decision sequence head (DataModel.DecisionSequenceHead)
    DecisionSequenceHeadCid: ContentId
}

/// Represents the result of an action execution by a broker
type ActionExecution = {
    /// Cid to the newly created execution sequence head (DataModel.ExecutionSequenceHead)
    ExecutionSequenceHeadCid: ContentId
}

/// Broadcasted by persistence system for other nodes to eagerly receive data referenced in other messages.
type PersistencePut = {
    Cid: ContentId
    Content: byte array
    TDatum: System.Type
}
