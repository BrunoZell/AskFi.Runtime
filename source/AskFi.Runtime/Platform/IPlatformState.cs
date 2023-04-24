namespace AskFi.Runtime.Platform;

public interface IPlatformState
{
    public Task Write(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value);
    public Task<ReadOnlyMemory<byte>> Read(ReadOnlySpan<byte> key);
}