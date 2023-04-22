namespace AskFi.Runtime.Modules.Perspective;

public interface IPerspectiveSource
{
    public IAsyncEnumerable<Sdk.Perspective> StreamPerspectives();
}
