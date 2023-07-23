module AskFi.Runtime.Index

open AskFi.Runtime.Persistence

type ContextSequenceIndex = {
    /// Node -> Ordinal
    Positioning: Map<ContentId (*ContextSequenceHead.Context*), uint64>

    /// Ordinal -> Node
    /// [Ordinal -> Node -> Actual Timestamp]
    Pointer: Map<uint64, ContentId (*ContextSequenceHead.Context*)>

    /// Actual Timestamp -> Ordinal of node at that timestamp
    Timestamps: Map<System.DateTime, uint64>
}

type IndexPool = {
    ContextSequences: Map<ContentId (*ContextSequenceHead.Identity*), ContextSequenceIndex>
    //ObservationSequences: Map<ContentId (*ObservationSequenceHead.Identity*), ObservationSequenceIndex>
}
