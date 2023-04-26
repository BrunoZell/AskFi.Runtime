module AskFi.Runtime.DataModel

open AskFi
open AskFi.Runtime.Persistence
open System

// ############################
// #### OBSERVATION MODULE ####
// ############################

/// Generated immediately after an IObserver emitted a new observation.
type CapturedObservation<'Perception> = {
    /// Absolute timestamp of when this observation was recorded.
    /// As of runtime clock.
    At: DateTime

    /// All perceptions that appeared at this instant, as emitted by an IObserver<'Perception> instance.
    Observation: Sdk.Observation<'Perception>
}

/// Generated sequentially within an Observer Module to add relative time relations.
type LinkedObservation = {
    Observation: ContentId // CapturedObservation<'Perception>

    /// Introduces relative ordering between CapturedObservations within an Observer Module
    Links: RelativeTimeLink array
}
and RelativeTimeLink = {
    /// Links to a LinkedObservation that happened before the link-owning observation.
    Before: ContentId
}

// ##############################
// ####  PERSPECTIVE MODULE  ####
// ##############################

/// A set of LinkedObservations are then merged into a Perspective Sequence.
/// This defines a temporal ordering between observations from different IObserver-instances and Observer Modules.
type PerspectiveSequenceHead =
    | Beginning
    | Happening of Node:PerspectiveSequenceNode
and PerspectiveSequenceNode = {
    /// Links previous PerspectiveSequenceHead to form a temporal order.
    Previous: ContentId // PerspectiveSequenceHead

    /// Cid to the then latest LinkedObservation that caused this update in perspective.
    LinkedObservation: ContentId // LinkedObservation
}

// ###########################
// ####  STRATEGY MODULE  ####
// ###########################

and ActionInitiation = {
    /// The 'Action from IBroker<'Action> (type of the originating observer instance)
    ActionType: Type
    /// Cid to the action information. Has type of ActionType.
    ActionCid: ContentId
}

type ActionSet = {
    /// All actions the strategy has decided to initiate.
    /// Those are keyed by 'ActionId'.
    Initiations: ActionInitiation array
}

type DecisionSequenceHead =
    | Start
    | Initiative of DecisionSequenceNode
and DecisionSequenceNode = {
    /// Links previous decision. This sequencing creates a temporal order between all decisions in this session.
    Previous: ContentId // DecisionSequenceHead

    /// What actions have been decided on.
    ActionSet: ContentId // ActionSet
}

// ############################
// ####  EXECUTION MODULE  ####
// ############################

type ActionExecutionTrace =
    /// Data emitted by the IBroker action execution. Could include an execution id, transaction, or validity proofs.
    | Success of trace: byte[] option
    /// IBroker action execution failed. This holds an exception message, if any, encountered during user code execution.
    | Error of ``exception``: string option

type ExecutionSequenceHead =
    | Start
    | Execution of ExecutionSequenceNode
and ExecutionSequenceNode = {
    /// Links previous decision. This sequencing creates a temporal order between all decisions in this session.
    Previous: ContentId // ExecutionSequenceHead
    
    /// What action has been executed.
    Action: ActionInitiation

    /// Trace output from broker.
    Trace: ActionExecutionTrace
}
