module AskFi.Runtime.DataModel

open AskFi
open AskFi.Runtime.Persistence
open System

// ############################
// #### OBSERVATION MODULE ####
// ############################

/// Generated immediately after an IObserver emitted a new observation grouping
/// the observation with the latest local timestamp as of the runtime clock.
type CapturedObservation<'Percept> = {
    /// Absolute timestamp of when this observation was recorded.
    /// As of runtime clock.
    At: DateTime

    /// All percepts that appeared at this instant, as emitted by an IObserver<'Percept> instance.
    Observation: Sdk.Observation<'Percept>
}

/// All captured obervations within an observer group are sequenced into
/// an observation sequence. Isolated observation sequences are a form of
/// entry point for new information into the system. Cids to such sequences
/// are passed arround to share information.
type ObservationSequenceHead =
    | Beginning
    | Happening of Node:ObservationSequenceNode
and ObservationSequenceNode = {
    /// Links previous ObservationSequenceHead to form a temporal order.
    Previous: ContentId // ObservationSequenceHead

    /// Cid to the then latest CapturedObservation<'Percept> that caused this update in perspective.
    Observation: ContentId // CapturedObservation
}

// ##############################
// ####  PERSPECTIVE MODULE  ####
// ##############################

/// A set of observation sequence heads are then merged into a perspective sequence.
/// This defines a temporal ordering between observations from different IObserver-instances and Observer Modules.
type PerspectiveSequenceHead =
    | Beginning
    | Happening of Node:PerspectiveSequenceNode
and PerspectiveSequenceNode = {
    /// Links previous PerspectiveSequenceHead to form a temporal order.
    Previous: ContentId // PerspectiveSequenceHead

    /// Cid to the then latest observation that added information to the previous perspective.
    /// It is referenced by the observation sequence head it was recorded in.
    LatestObservation: ContentId // ObservationSequenceHead
}

/// A cluster-wide CRDT where all observations are merged into to form
/// an ever-growing pool of observations.
type ObservationPool = {
    AggregatePerspective: ContentId
    DroppedPerspectives: ContentId Set
}

// ###########################
// ####  STRATEGY MODULE  ####
// ###########################

type ActionSet = {
    /// All actions the strategy has decided to initiate.
    /// Those are keyed by 'ActionId'.
    Initiations: ActionInitiation array
}
and ActionInitiation = {
    /// The 'Action from IBroker<'Action> (type of the originating observer instance)
    ActionType: Type
    /// Cid to the action information. Has type of ActionType.
    ActionCid: ContentId
}

type BacktestEvaluationHead =
    | Start of BacktestEvaluationStart
    | Initiative of BacktestEvaluationNode
and BacktestEvaluationStart = {
    /// Links to first and latest perspective the backtest ran on.
    LastPerspective: ContentId // PerspectiveSequenceHead
}
and BacktestEvaluationNode = {
    /// Links previous backtest evaluation.
    Previous: ContentId // BacktestEvaluationStart

    /// What actions have been decided on by the backtested strategy.
    ActionSet: ContentId // ActionSet
}

type LiveEvaluationHead =
    | Start of LiveEvaluationStart
    | Initiative of LiveEvaluationNode
and LiveEvaluationStart = {
    /// Links to first and earliest perspective this live execution ran on.
    FirstPerspective: ContentId // PerspectiveSequenceHead
}
and LiveEvaluationNode = {
    /// Links previous backtest evaluation.
    Previous: ContentId // LiveEvaluationHead

    /// What actions have been decided on by the executing strategy.
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

type ActionExecutionResult = {
    /// Trace output from broker.
    Trace: ActionExecutionTrace
    
    /// When the used IBroker implementation started executing.
    InitiationTimestamp: DateTime

    /// When the used IBroker implementation completed executing.
    CompletionTimestamp: DateTime
}

type ExecutionSequenceHead =
    | Start
    | Execution of Node:ExecutionSequenceNode
and ExecutionSequenceNode = {
    /// Links previous decision. This sequencing creates a temporal order between all decisions in this session.
    Previous: ContentId // ExecutionSequenceHead
    
    /// What action has been executed.
    Action: ActionInitiation
}
