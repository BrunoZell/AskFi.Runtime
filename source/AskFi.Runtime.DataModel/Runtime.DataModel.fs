module AskFi.Runtime.DataModel

open System

// ############################
// #### OBSERVER SUBSYSTEM ####
// ############################

/// Groups perceptions that happened at the same instant. In this case, there must be no order defined
/// to unabiguously compute over that data later on.
/// This typically happens when there are multiple perceptions sourced from a single received network message.
type AtomicObservation<'Perception> =
    | SensoryInformation of System.ReadOnlyMemory<'Perception>

/// Raw data structure produced by an Observer instance.
/// All new information received via this Observer instance is referenced in this tree.
type ObservationSequenceHead<'Perception> =
    | Beginning
    | Observation of Observation<'Perception>
and Observation<'Perception> = {
    /// All observations that happened at this instant.
    Observation: AtomicObservation<'Perception>

    /// Link to the previous observations of this session, forming a linked list and sequencing them.
    /// If this is the first observation of this session, this links to the 'Beginning' union case.
    Previous: ObservationSequenceHead<'Perception>
}

// ###########################
// #### SESSION SUBSYSTEM ####
// ###########################

type Timestamp = DateTime

/// The hash that's used in CIDs referring to an instance of PerspectiveSequenceHead.
/// It's a 32 bit signed integer (for now) to be compatible with .NETs object.GetHashCode().
type PerspectiveHash = int32

/// Updates to multiple Observation Sequences are sequenced with each other into a Perspective Sequence.
/// This defines an ordering between observations from different Observation Sequences (and implicitly, different IObserver-instances)
/// and merges them into a single sequence of observations (accorss all Perception-types).
type PerspectiveSequenceHead =
    | Empty
    | Happening of at:Timestamp (*as of runtime clock*) * previous:PerspectiveHash * observationStreamHead:obj // actually ObservationSequenceHead<_> of all possible types. Todo: implement as recursion scheme
