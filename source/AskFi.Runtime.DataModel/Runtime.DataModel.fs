module AskFi.Runtime.DataModel

open AskFi.Runtime.Persistence
open System

// ######################
// #### OBSERVATIONS ####
// ######################

/// Generated immediately after an IObserver emitted a new observation grouping
/// the observation with the latest local timestamp as of the runtime clock.
type CapturedObservation = {
    /// Absolute timestamp of when this observation was recorded.
    /// As of runtime clock.
    At: DateTime

    /// The 'Percept from Observation<'Percept> (type of the originating observer instance)
    PerceptType: Type

    /// All percepts that appeared at this instant, as emitted by an IObserver<'Percept> instance.
    Observation: ContentId // Sdk.Observation<'Percept>
}

/// All captured observations within an observer group are sequenced into
/// an observation sequence. Isolated observation sequences are a form of
/// entry point for new information into the system. Cids to such sequences
/// are passed around to share information.
type ObservationSequenceHead =
    | Identity of Nonce:uint64
    | Observation of Node:ObservationSequenceNode
and ObservationSequenceNode = {
    /// Links previous ObservationSequenceHead to form a temporal order.
    Previous: ContentId // ObservationSequenceHead

    /// Cid to the then latest CapturedObservation that caused this update in perspective.
    Observation: ContentId // CapturedObservation
}

// #####################
// ####  EXECUTION  ####
// #####################

type ActionSet = {
    /// All actions the strategy has decided to initiate.
    Actions: Action array
}
and Action = {
    /// 'Action to route to according IBroker<'Action>.
    /// This type is taken from what the strategy emitted in its decision.
    ActionType: Type

    /// Cid to the action information. Has type of ActionType.
    ActionCid: ContentId
}

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

/// An execution sequence is produced by a Broker Group, which forms the second type
/// of data entry into the system, holding information we got from executing actions.
type ExecutionSequenceHead =
    | Identity of Nonce:uint64
    | Execution of Node:ExecutionSequenceNode
and ExecutionSequenceNode = {
    /// Links previous decision.
    Previous: ContentId // ExecutionSequenceHead

    /// What actions have been executed.
    Executed: ActionSet
}

/// A cluster-wide CRDT where all action executions are merged into to form
/// an ever-growing pool of trace information.
type ActionExecutionPool = {
    AggregateExecutionSequence: ContentId // ExecutionSequenceHead
}

// ##################
// ####  MEMORY  ####
// ##################

/// With observation sequences and execution sequences being the root data entry point of all external data
/// flowing into the system, a knowledge base instance references a specific set of those sequences.
/// When two observation pools are merged, their greatest sum is taken, i.e. that with most information,
/// what include all information from both.
/// Each sequence is identified by its very first root node, and is then mapped to all latest known sequence heads sharing the same first node.
/// If all producing observers and brokers where honest and bug free, then there always would be one latest version. However, in case there are
/// multiple competing versions for the same sequence head, all will be referenced by this data structure to not loose data.
/// This is fine in a permissioned environment.
type KnowledgeBase = {
    /// Maps ObservationSequenceHead.Identity as the observation sequence id
    /// to the latest known ObservationSequenceHead.Observation
    IncludedObservationSequences: Map<ContentId, ContentId list> // Map<ObservationSequenceHead, ObservationSequenceHead list>
    
    /// Maps ExecutionSequenceHead.Identity as the observation sequence id
    /// to the latest known ExecutionSequenceHead.Execution
    IncludedExecutionSequences: Map<ContentId, ContentId list> // Map<ExecutionSequenceHead, ExecutionSequenceHead list>
}

// ######################
// ####  STRATEGIES  ####
// ######################

/// Decision sequence for strategy executions along a perspective sequence, where
/// decisions are made from now into the past.
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

/// Decision sequence for strategy executions on a live observation stream, where
/// decisions are made from now into the future.
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
