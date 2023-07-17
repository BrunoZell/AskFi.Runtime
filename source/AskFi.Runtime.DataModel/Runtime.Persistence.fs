namespace AskFi.Runtime.Persistence

open System.Runtime.CompilerServices

// Todo: Make this an IPFS CID respecting multihash and multicodec
[<IsReadOnly; Struct>]
type ContentId = ContentId of Raw: byte array with
    static member Zero
        with get() = ContentId <| Array.zeroCreate<byte> 0
