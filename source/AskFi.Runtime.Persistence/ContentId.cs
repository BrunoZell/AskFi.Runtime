namespace AskFi.Runtime.Persistence;

public readonly struct ContentId
{
    public ContentId(byte[] raw)
    {
        Raw = raw;
    }

    // Todo: Make this an IPFS CID respecting multihash and multicodec
    public byte[] Raw { get; }
}
