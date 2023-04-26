# Runtime Internals

## Modules

The _AskFi Runtime_ is seperated into modules that solve distinct problems individually and can be composed together to support a variety of [_Platform Apps_](#platform-apps).

Modules are .NET types that are initialized with:

- A reference to `IPlatformPersistence` to read or write persistent storage
- A reference to `ChannelReader<TInput>` to receive input messages from
- User-defined configuration (like dlls with SDK implementations)

Once initialized, they can be composed with other modules by passing their `.Output` property into the initialization of another module.

To start the module pipeline, call `Run(cancellationToken)` on all of the individual modules.

To stop the pipeline, cancel the passed `cancellationToken`.

List of runtime modules:

- [Observer Module](modules/observer.md)
- [Perspective Module](modules/perspective.md)
- Perspective Merge Module
- Strategy Module
- Execution Module

## Modes

- [Scraper](modes/scraper.md)
- [Sequencer](modes/sequencer.md)
- [Evaluator](modes/evaluator.md)
- [Executor](modes/executor.md)
- [Visualizer](modes/visualizer.md)
