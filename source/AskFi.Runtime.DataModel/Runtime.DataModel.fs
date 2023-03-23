module AskFi.Runtime.DataModel

open System

type ObservationStreamHead<'Perception> =
    | Beginning
    | Observation of Observation<'Perception>
and Observation<'Perception> = {
    /// All observations that happened at this instant.
    /// (Possible multiple if there are more than one sensory event in a single received network message).
    Observations: System.ReadOnlyMemory<'Perception>

    /// Link to the observation session this observation is part of.
    /// If this is the first observation, this links to the session info.
    /// For all consecutive observations, this links to the previous observation, forming a linked list.
    Previous: ObservationStreamHead<'Perception>
}

type Timestamp = DateTime
type WorldEventSequenceHash = int32 // Actually bytes32 or some well known hash that's used for CIDs to the world event sequence heads
/// Every session sequences observations from all IObserver-instances into a single sequence of observations (accorss all Perception-types)
type WorldEventSequence =
    | Empty
    /// nonce: A field with arbitrary data to use in case of a hash collision within the same WorldEventSequence (must be unique to form a valid linked list) but small enough to not be too computationally heavy.
    | Happening of at:Timestamp (*as of runtime clock*) * previous:WorldEventSequenceHash (*hash of WorldEventSequence*) * nonce:uint64 * observationStreamHead:obj // actually ObservationStreamHead<_> of all possible types. Todo: implement as recursion scheme
