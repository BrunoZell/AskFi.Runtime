module AskFi.Runtime.DataModel

open AskFi
open System

// ############################
// #### OBSERVER SUBSYSTEM ####
// ############################

/// Raw data structure produced by an Observer instance.
/// All new information received via this Observer instance is referenced in this tree.
type ObservationSequenceHead<'Perception> =
    | Beginning
    | Observation of ObservationSequenceNode<'Perception>
and ObservationSequenceNode<'Perception> = {
    /// All observations that happened at this instant.
    Observation: Sdk.Observation<'Perception>

    /// Link to the previous observations of this session, forming a linked list and sequencing them.
    /// If this is the first observation of this session, this links to the 'Beginning' union case.
    Previous: ObservationSequenceHead<'Perception>
}

// Todo: Add index trees: ChronologicalObservationSequence, ContinuousObservationSequence, ChronologicalContinuousObservationSequence

// ###########################
// #### SESSION SUBSYSTEM ####
// ###########################

type Timestamp = DateTime

/// The hash that's used in CIDs referring to an instance of PerspectiveSequenceHead.
/// It's a 32 bit signed integer (for now) to be compatible with .NETs object.GetHashCode().
type PerspectiveHash = int32

/// Updates to multiple Observation Sequences are sequenced with each other into a Perspective Sequence.
/// This defines an ordering between observations from different Observation Sequences (and implicitly, different IObserver-instances)
/// and merges them into a single sequence of observations (across all Perception-types).
type PerspectiveSequenceHead =
    | Empty
    | Happening of at:Timestamp (*as of runtime clock*) * previous:PerspectiveHash * observationStreamHead:obj // actually ObservationSequenceHead<_> of all possible types. Todo: implement as recursion scheme
