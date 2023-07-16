# Runtime Mode: Sequencer

## Processing Pipeline

### Observation pool gossip

1. Input `NewObservationPool`
2. Observation Pool Merge Module
3. Output `NewObservationPool`

### Active onboarding of new observations

1. Input `NewObservation`
2. Convert `NewObservation` into `ObservationPool`
3. Observation Pool Merge Module
4. Output `NewObservationPool`

## Message Usage

| Runtime Message Type          | listening | broadcasting |
| ----------------------------- | --- | --- |
| `NewObservationPool`          | ✅ | ✅ |
| `NewObservation`              | ✅ |     |
| `PersistencePut`              | ✅ | ✅ |

## Persistence Usage

| Type                          | cid | get | put | pin |
| ----------------------------- | --- | --- | --- | --- |
| `ObservationPool`             | ✅  | ✅ |     | ✅ |
| `ObservationSequenceHead`     | ✅  | ✅ |     | ✅ |
| `ObservationSequenceNode`     | ✅  | ✅ |     | ✅ |
| `CapturedObservation`         | ✅  | ✅ |     | ✅ |
