# AskFi Runtime

The _AskFi Runtime_ is a library of composable modules that:

- Listen to input messages and produce output messages. All messages produced and consumed are defined in [`Runtime.Messages.fs`](../AskFi.Runtime.DataModel/Runtime.Messages.fs).
- Put, Get, or Cid Runtime DataModel types. All those persistable and (through messages) transmittable data types are defined in [`Runtime.DataModel.fs`](../AskFi.Runtime.DataModel/Runtime.DataModel.fs).
- Take implementations of domains as custom types that build on the [`AskFi.Sdk.fs`](../../sdk/source/AskFi.Sdk.fs) to parameterize computation (interpretation and strategies) or to orchestrate network traffic (observers and brokers).

## Implemented Runtime Modules

- [Observer Module](Modules/Observation/observer.md)
- [Observation Integration Module](Modules/ObservationPool/observation-integration.md)
- [Observation Deduplication Module](Modules/ObservationPool/observation-deduplication.md)
- [Strategy Module](Modules/Strategy/strategy.md)
- [Execution Module](Modules/Execution/execution.md)

## Predefined Configurations

- [Scraper](scraper.md)
- [Observation Gossip](observation-gossip.md)
- [Live Strategy](live-strategy.md)
- [Broker](broker.md)

## Module Implementation Details

Modules are .NET types that are initialized with, respectively:

- A reference to `IPlatformPersistence` to read or write persistent storage
- A reference to `ChannelReader<TInput>` to receive input messages from
- User-defined configuration (like dlls with SDK implementations)

Once initialized, they can be composed with other modules by passing their `.Output` property into the input initialization of another module.

To start the module pipeline, call `Run(cancellationToken)` on all of the individual modules.

To stop the pipeline, cancel the passed `cancellationToken`.
