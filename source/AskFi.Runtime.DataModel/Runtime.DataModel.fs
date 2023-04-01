module AskFi.Runtime.DataModel

open AskFi
open System
open AskFi.Persistence

// ###############################
// #### OBSERVATION SUBSYSTEM ####
// ###############################

/// Raw data structure produced by an IObserver-instance.
/// All new information received via this IObserver-instance is referenced in this tree.
type ObservationSequenceHead<'Perception> =
    | Beginning
    | Observation of ObservationSequenceNode<'Perception>
and ObservationSequenceNode<'Perception> = {
    /// All perceptions that appeared at this instant.
    Observation: Sdk.Observation<'Perception>

    /// Link to the previous observations of this session, forming a linked list and sequencing them.
    /// If this is the first observation of this session, this links to the 'Beginning' union case.
    Previous: ObservationSequenceHead<'Perception>
}

// Todo: Add index trees: ChronologicalObservationSequence, ContinuousObservationSequence, ChronologicalContinuousObservationSequence

/// Updates to multiple Observation Sequences are sequenced with each other into a Perspective Sequence.
/// This defines an ordering between observations from different Observation Sequences (and implicitly, different IObserver-instances)
/// and merges them into a single sequence of observations (across all Perception-types).
type PerspectiveSequenceHead =
    | Empty
    | Happening of PerspectiveSequenceNode
and PerspectiveSequenceNode = {
    /// Absolute timestamp of when this happening occurred.
    /// As of runtime clock.
    At: DateTime

    /// Links previous PerspectiveSequenceHead. This sequencing creates a temporal order between IObserver-instances.
    Previous: ContentId // PerspectiveSequenceHead

    /// Link to the updated ObservationSequenceHead<_> that caused this update in perspective.
    /// ObservationSequenceHead<_> of all possible types.
    /// Todo: implement as recursion scheme
    ObservationStreamHead: ContentId // ObservationSequenceHead<_>
}
