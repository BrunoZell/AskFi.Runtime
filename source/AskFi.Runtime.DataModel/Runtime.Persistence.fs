namespace AskFi.Runtime.Persistence

open System.Runtime.CompilerServices

[<IsReadOnly; Struct>]
type ContentId = {
    // Todo: Make this an IPFS CID respecting multihash and multicodec
    Raw: byte array
}
