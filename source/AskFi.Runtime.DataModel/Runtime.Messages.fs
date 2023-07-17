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

/// Nodes send this message to gossip the latest knowledge base.
/// Knowledge base gossiper send them out when they received a NewObservation message.
type NewKnowledgeBase = {
    /// Cid to the latest heaviest knowledge base
    KnowledeBase: ContentId // KnowledeBase
}

/// Message emitted from Strategy Module when a non-inaction decision has been made
type NewDecision = {
    /// Cid to the original DecisionSequenceHead.Identity of the newly produced decision sequence head.
    Identity: ContentId

    /// Cid to the newly created decision sequence head (DataModel.DecisionSequenceHead)
    Head: ContentId
}

/// Represents the result of a completed action execution, successful or not, by a broker
type ActionExecuted = {
    /// Cid to the original ActionSequenceHead.Identity of the newly produced action sequence head.
    Identity: ContentId

    /// Cid to the newly created action sequence head (DataModel.ActionSequenceHead)
    Head: ContentId
}

/// Broadcasted by persistence system for other nodes to eagerly receive data referenced in other messages.
type PersistencePut = {
    Cid: ContentId
    Content: byte array
    TDatum: System.Type
}
