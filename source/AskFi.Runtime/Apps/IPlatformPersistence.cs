using AskFi.Persistence;

namespace AskFi.Runtime.Apps;

public interface IPlatformPersistence
{
    ContentId Cid<TDatum>(TDatum datum);
    ValueTask<TDatum> Get<TDatum>(ContentId contentId);
    ValueTask<ContentId> Put<TDatum>(TDatum datum);
    ValueTask<bool> Pin(ContentId contentId);
}
