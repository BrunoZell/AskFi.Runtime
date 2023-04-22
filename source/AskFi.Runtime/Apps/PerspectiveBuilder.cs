using System.Collections.Concurrent;
using System.Collections.Immutable;
using AskFi.Persistence;

namespace AskFi.Runtime.Modes;

public class PerspectiveBuilder
{
    private readonly ConcurrentDictionary<Type, ImmutableHashSet<ContentId>> _observations = new();

    public void WithObservation<TPerception>(ContentId observationCid)
    {
        _observations.AddOrUpdate(
            key: typeof(TPerception),
            addValueFactory: (key, cid) => ImmutableHashSet.Create(cid),
            updateValueFactory: (key, existing, cid) => existing.Add(cid),
            factoryArgument: observationCid);
    }

    public Sdk.Perspective Build()
    {
        // Todo: Merge all observations into a Perspective
        return new Sdk.Perspective();
    }
}
