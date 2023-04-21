# Runtime Internals

All persistent or transmitted data in the system is _content addressable_. Which esentially is just to identify a given piece of data by its hash. For this, the Runtime adheres to the specs of [IPFS](https://docs.ipfs.tech/) and [IPLD](https://ipld.io/docs/).

## Modules

The _AskFi Runtime_ is seperated into modules that solve distinct problems individually and can be composed together to support a variety of [_Runtime Modes_](#modes).

- [Observer Group Module](modules/observer-group.md)
- [Perspective Module](modules/perspective.md)
- [Scene Module] get (Perspective), cid, put (Scene)
- [Strategy Module]: get (Perspective, Scene), cid, put (Decision), pin
- [Execution Module]: get (Decision), cid, put (Trace), pin

## Modes

### Scraper

Operating _Observation Module_.

### Analyze, Visualize

Operating _Perspective Module_ and _Scene Module_ to then run custom code on `Perspective` and `Scene`.

### Inaction Expectations

### Strategy Backtest (action expectations)

### Live Strategy

### Broker (action execution)
