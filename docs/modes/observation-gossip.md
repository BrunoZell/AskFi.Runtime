# Runtime Mode: Observation Gossip

## Processing Pipeline

### Active onboarding of new observations

1. Input `NewObservation`
2. Observation Integration Module
3. Output `NewObservationPool`

## Message Usage

| Runtime Message Type          | listening | broadcasting |
| ----------------------------- | --- | --- |
| `NewObservationPool`          | ✅ | ✅ |
