module AskFi.Runtime.DataModel

open AskFi
open AskFi.Persistence
open AskFi.Sdk
open System
open System.Runtime.CompilerServices

// ###############################
// #### OBSERVATION SUBSYSTEM ####
// ###############################

/// Generated immediately after an IObserver emitted a new observation.
type CapturedObservation<'Perception> = {
    /// Absolute timestamp of when this observation was recorded.
    /// As of runtime clock.
    At: DateTime

    /// All perceptions that appeared at this instant.
    Observation: Sdk.Observation<'Perception>
}

/// Generated sequentially within an ObserverGroup to add relative time relations.
type LinkedObservation = {
    Observation: ContentId // CapturedObservation<'Perception>
    Links: RelativeTimeLink array // To introduce relative ordering within an ObserverGroup
}
and RelativeTimeLink = {
    Before: ContentId // Links to a LinkedObservation that happened before the link-owning observation.
}

/// Updates to multiple Observation Sequences are sequenced with each other into a Perspective Sequence.
/// This defines an ordering between observations from different Observation Sequences (and implicitly, different IObserver-instances)
/// and merges them into a single sequence of observations (across all Perception-types).
type ObservationGroupSequenceHead =
    | Beginning
    | Happening of Node:ObservationGroupSequenceNode
and ObservationGroupSequenceNode = {
    /// Links previous ObservationGroupSequenceHead. This sequencing ensures all observations produced by this observer group are kept as references even if single head updates are dropped.
    Previous: ContentId // ObservationGroupSequenceHead

    // Todo: implement as recursion scheme
    /// Cid to the newest LinkedObservation that caused this update in perspective.
    LinkedObservation: ContentId // LinkedObservation
}

// ##############################
// ####  STRATEGY SUBSYSTEM  ####
// ##############################

type DecisionSequenceHead =
    | Start
    | Initiative of DecisionSequenceNode
and DecisionSequenceNode = {
    /// All actions the strategy has decided to initiate.
    /// Those are keyed by 'ActionId'.
    ActionSet: ActionInitiation array

    /// Link to the updated perception that caused the strategy to execute and produce a decision.
    PerspectiveSequenceHead: ContentId // PerspectiveSequenceHead<_>

    /// Absolute timestamp (as of runtime clock) of when the decision was made. This is the timestamp _after_ the strategy-code has
    /// been fully executed. So this is equal to 'this.PerspectiveSequenceHead.At + runtime(strategy(this.PerspectiveSequenceHead))'.
    At: DateTime

    /// Links previous decision. This sequencing creates a temporal order between all decisions in this session.
    Previous: ContentId // DecisionSequenceHead
}

/// Each action initiation is identified by the content ID of the related Session Sequence Head
/// plus a zero-based numeric index into ActionSet[i].
/// This helps to analyze logs and disambiguate actions that are otherwise exactly equal.
[<IsReadOnly; Struct>]
type ActionId = {
    DecisionSequenceHead: ContentId
    ActionIndex: int32
}
