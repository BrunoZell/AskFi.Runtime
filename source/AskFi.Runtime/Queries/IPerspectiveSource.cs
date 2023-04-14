namespace AskFi.Runtime.Queries;

public interface IPerspectiveSource
{
    public IAsyncEnumerable<Sdk.Perspective> StreamPerspectives();
}
