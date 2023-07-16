# Runtime Mode: Evaluator

Operating _Perspective Module_, _Context Module_, and _Strategy Module_ to run custom strategies in the form of `type Decide = Reflection -> Perspective -> Decision`.

| Type                          | cid | get | put | pin |
| ----------------------------- | --- | --- | --- | --- |
| `CapturedObservation`         |     | ✅  |    |      |
| `LinkedObservation`           |     | ✅  |    |      |
| `RelativeTimeLink`            |     | ✅  |    |      |
| `PerspectiveSequenceHead`     |     | ✅ |     |      |
| `PerspectiveSequenceNode`     |     | ✅ |     |      |
| `DecisionSequenceHead`        | ✅  |     | ✅ | ✅  |
| `DecisionSequenceNode`        | ✅  |     | ✅ | ✅  |
| `ActionSet`                   | ✅  |     | ✅ | ✅  |
