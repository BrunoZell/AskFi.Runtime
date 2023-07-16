# Runtime Mode: Live Strategy

Run custom strategies in the form of `type Decide = Reflection -> Perspective -> Decision` against newest versions of the observation pool.

## Processing Pipeline

1. Input `NewObservationPool`
2. Observation Deduplication Module
3. Strategy Module
4. Output `NewDecision`
