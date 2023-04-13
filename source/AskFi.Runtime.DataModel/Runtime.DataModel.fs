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
    | Observation of Node:ObservationSequenceNode<'Perception>
and ObservationSequenceNode<'Perception> = {
    /// Absolute timestamp of when this observation was recorded.
    /// As of runtime clock.
    At: DateTime

    /// All perceptions that appeared at this instant.
    Observation: Sdk.Observation<'Perception>

    /// Link to the previous observations of this session, forming a linked list and sequencing them.
    /// If this is the first observation of this session, this links to the 'Beginning' union case.
    Previous: ContentId // ObservationSequenceHead<'Perception>
}

// Todo: Add index trees: ChronologicalObservationSequence, ContinuousObservationSequence, ChronologicalContinuousObservationSequence

/// Updates to multiple Observation Sequences are sequenced with each other into a Perspective Sequence.
/// This defines an ordering between observations from different Observation Sequences (and implicitly, different IObserver-instances)
/// and merges them into a single sequence of observations (across all Perception-types).
type PerspectiveSequenceHead =
    | Empty
    | Happening of Node:PerspectiveSequenceNode
and PerspectiveSequenceNode = {
    /// Absolute timestamp of when this happening occurred.
    /// As of runtime clock.
    At: DateTime

    /// Links previous PerspectiveSequenceHead. This sequencing creates a temporal order between IObserver-instances.
    Previous: ContentId // PerspectiveSequenceHead

    // Todo: implement as recursion scheme
    /// Link to the updated ObservationSequenceHead<_> that caused this update in perspective.
    /// ObservationSequenceHead<_> of all possible types.
    ObservationSequenceHead: ContentId // ObservationSequenceHead<_>

    // Todo: Make a serializable IPLD Link<T> structure to embed type info in the types itself.
    /// Type 'P of ObservationSequenceHead<'P> linked above.
    ObservationPerceptionType: Type
}
