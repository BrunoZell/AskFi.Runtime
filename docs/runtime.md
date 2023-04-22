# Runtime Internals

All persistent or transmitted data in the system is _content addressable_. Which esentially is just to identify a given piece of data by its hash. For this, the Runtime adheres to the specs of [IPFS](https://docs.ipfs.tech/) and [IPLD](https://ipld.io/docs/).

## Modules

The _AskFi Runtime_ is seperated into modules that solve distinct problems individually and can be composed together to support a variety of [_Platform Apps_](#platform-apps).

- [Observer Module](modules/observer.md)
- [Perspective Module](modules/perspective.md)
- [Scene Module]
- [Strategy Module]
- [Execution Module]

## Platform Apps

The AskFi Platform is an additional layer of abstraction that introduces the benefits of the AskFi SDK + Runtime in a managed way, by implementing the necessary infrastructure required to offer it as a hosted service.

### Scraper

Operating _Observation Module_ on custom `IObserver`-Instances.

| Type                          | cid | get | put | pin |
| ----------------------------- | --- | --- | --- | --- |
| `CapturedObservation`         | ✅  |     | ✅ |  ✅  |
| `LinkedObservation`           | ✅  |     | ✅ |  ✅  |
| `RelativeTimeLink`            | ✅  |     | ✅ |  ✅  |

### Analyze

Operating _Perspective Module_ and _Scene Module_ to then run custom code in the form of `(Perspective, Scene) -> unit`.

| Type                          | cid | get | put | pin |
| ----------------------------- | --- | --- | --- | --- |
| `PerspectiveSequenceHead`     |     | ✅  | ✅ |     |
| `PerspectiveSequenceNode`     |     | ✅  | ✅ |     |
| `Scene` (todo)                |     | ✅  | ✅ |     |

### Visualize

Operating _Perspective Module_, _Scene Module_ and _Strategy Module_ to produce a _Canvas_ to be visualized in the platforms visualization system.

| Type                          | cid | get | put | pin |
| ----------------------------- | --- | --- | --- | --- |
| `CapturedObservation`         |     | ✅  |    |      |
| `LinkedObservation`           |     | ✅  |    |      |
| `RelativeTimeLink`            |     | ✅  |    |      |
| `PerspectiveSequenceHead`     |     | ✅ |     |      |
| `PerspectiveSequenceNode`     |     | ✅ |     |      |
| `DecisionSequenceHead`        |     | ✅ |     |      |
| `DecisionSequenceNode`        |     | ✅ |     |      |
| `Canvas`                      |     |     | ✅ |  ✅  |

### Live Strategy

Operating _Perspective Module_, _Scene Module_, and _Strategy Module_ to run custom strategies in the form of `type Decide = Reflection -> Perspective -> Decision`.

| Type                          | cid | get | put | pin |
| ----------------------------- | --- | --- | --- | --- |
| `CapturedObservation`         |     | ✅  |    |      |
| `LinkedObservation`           |     | ✅  |    |      |
| `RelativeTimeLink`            |     | ✅  |    |      |
| `PerspectiveSequenceHead`     |     | ✅ |     |      |
| `PerspectiveSequenceNode`     |     | ✅ |     |      |
| `Scene` (todo)                |     | ✅ |     |      |
| `DecisionSequenceHead`        | ✅  |     | ✅ | ✅  |
| `DecisionSequenceNode`        | ✅  |     | ✅ | ✅  |

### Broker

Action execution from _Live Strategy_ by running _Execution Module_.

| Type                          | cid | get | put | pin |
| ----------------------------- | --- | --- | --- | --- |
| `DecisionSequenceHead`        |     | ✅  |    |     |
| `DecisionSequenceNode`        |     | ✅  |    |     |

## Platform Infrastructure

Internally, the platform executes [_Runtime Modes_](#modules) within the platforms infrastructure:

- Low-latency messaging (Redis)
- Persistence (IPFS Cluster)
- State (etcd)
- Container Scheduling, Networking (Kubernetes), specifically for all _Runtime Modes_ and _Platform Services_.
