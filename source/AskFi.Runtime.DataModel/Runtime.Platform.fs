namespace AskFi.Runtime.Platform

open System.Threading.Tasks
open System
open System.Collections.Generic
open System.Threading
open AskFi.Runtime.Persistence

type IPlatformState =
    abstract member Write: ReadOnlySpan<byte> * ReadOnlySpan<byte> -> Task
    abstract member Read: ReadOnlySpan<byte> -> Task<ReadOnlyMemory<byte>>

type IPlatformPersistence =
    abstract member Cid<'TDatum> : 'TDatum -> ContentId
    abstract member Get<'TDatum> : ContentId -> ValueTask<'TDatum>
    abstract member Put<'TDatum> : 'TDatum -> ValueTask<ContentId>
    abstract member Pin : ContentId -> ValueTask<bool>

type IPlatformMessaging =
    abstract member Emit<'TMessage> : 'TMessage -> unit
    abstract member Listen<'TMessage> : CancellationToken -> IAsyncEnumerable<'TMessage>
