namespace AskFi.Runtime

open AskFi.Runtime.Messages
open System.Threading.Channels

// This file defined the message interface of each runtime module.

type IObserverModule =
    abstract member Output : ChannelReader<NewObservation>

type IPerspectiveModule =
    abstract member Input : ChannelWriter<NewObservation>
    abstract member Output : ChannelReader<NewPerspective>

type IPerspectiveMergeModule =
    abstract member Input : ChannelWriter<NewPerspective>
    abstract member Output : ChannelReader<NewPerspective>

type IStrategyModule =
    abstract member Input : ChannelWriter<NewPerspective>
    abstract member Output : ChannelReader<NewDecision>

type IExecutionModule =
    abstract member Input : ChannelWriter<NewDecision>
    abstract member Output : ChannelReader<ActionExecuted>
