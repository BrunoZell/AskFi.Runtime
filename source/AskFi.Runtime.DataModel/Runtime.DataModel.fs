module AskFi.Runtime.DataModel

open AskFi
open System
open AskFi.Persistence

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
type PerspectiveSequenceHead =
    | Empty
    | Happening of Node:PerspectiveSequenceNode
and PerspectiveSequenceNode = {
    /// Absolute timestamp of when the linked observation was recorded (first observation in ObservationSequenceHead).
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
