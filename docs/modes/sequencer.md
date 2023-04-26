# Runtime Mode: Sequencer

## Configuration

1. Input `NewPerspective`
2. Perspective Merge Module
3. Output `NewPerspective`

## Persistence Usage

| Type                          | cid | get | put | pin |
| ----------------------------- | --- | --- | --- | --- |
| `CapturedObservation`         |     | ✅  |    |      |
| `LinkedObservation`           |     | ✅  |    |      |
| `PerspectiveSequenceHead`     | ✅ | ✅   | ✅ | ✅  |
| `PerspectiveSequenceNode`     | ✅ | ✅   | ✅ | ✅  |
