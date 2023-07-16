# Observation Pool Module

## Runtime Data Types

### `CapturedObservation<'Percept>`

Generated immediately after an _`IObserver`-Instance_ emitted a new observation. An absolute timestamp of when this observation was recorded is attached. For that, the configured runtime clock. is used.

## Component Exeution

Observations from all Observer-instances are eagery pulled and turned into `CapturedObservation<_>` by attaching the current timestamp as by the runtime clock.
