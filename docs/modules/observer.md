# Observer Module

## SDK Types

### `IObserver<'Percept>`. `Observation<'Percept>`

An _Observation_ is an atomic appearance of sensory information. It consists of one or more _Percepts_, all of the same type. Percepts are domain-specific types to represent the newly observed information, sourced from the external world. _Observers_ sit at the oundary between the external world and the AskFi system.

An _Observation_ can have multiple _Percepts_ in case multiple perceptions appeared at the same instant (same point in time). An individual observation, by definition, appeared at a singular instant and thus the list of percepts within a single observation does not specify an order.

## Runtime Data Types

### `CapturedObservation<'Percept>`

Generated immediately after an _`IObserver`-Instance_ emitted a new observation. An absolute timestamp of when this observation was recorded is attached. For that, the configured runtime clock. is used.

### `LinkedObservation`, `RelativeTimeLink`

Generated sequentially within an ObserverGroup to add relative time relations.

Introduces relative ordering between CapturedObservations within an ObserverGroup

Links to a LinkedObservation that happened before the link-owning observation.

All LinkedObservations produced within a single ObserverGroup are sequenced into a ObservationGroupSequence.
This defines an ordering between observations from different IObserver-instances in addition to the absolute
timestamp which may not be exactl accurate.

## Component Exeution

An _Observer Group_ is initialized with:

- a set of `IObserver<_>` instances
- a Runtime clock,
- Peristence access of:
  - cid (CapturedObservation<_>)
  - put (CapturedObservation<_>, LinkedObservation)
  - pin

Observations from all Observer-instances are eagery pulled and turned into `CapturedObservation<_>` by attaching the current timestamp as by the Runtime clock.

On each new `CapturedObservation<_>`, a new `LinkedObservation` is created which links all most recent observations of other Observer-instances within the _Observer Group_ via a "$latest was observed before $new". This introduces relative time ordering for all Observations produced within an _Observer Group_. `LinkedObservations` are then peristed and pinned within the platform-wide _Observation Pool_ (which is a pinset of all archived `LinkedObservations` the platform-instance ever created).
