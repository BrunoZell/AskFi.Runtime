module AskFi.Runtime.DataModel

open System

type ObservationSequenceHead<'Perception> =
    | Beginning
    | Observation of Observation<'Perception>
and Observation<'Perception> = {
    /// All observations that happened at this instant.
    /// (Possible multiple if there are more than one sensory event in a single received network message).
    Observations: System.ReadOnlyMemory<'Perception>

    /// Link to the observation session this observation is part of.
    /// If this is the first observation, this links to the session info.
    /// For all consecutive observations, this links to the previous observation, forming a linked list.
    Previous: ObservationSequenceHead<'Perception>
}

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
