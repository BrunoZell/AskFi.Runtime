# Runtime Mode: Executor

## Processing Pipeline

1. Input `NewDecision`
2. Executor Module
3. Output `ActionExecution`

## Persistence Usage

| Type                          | cid | get | put | pin |
| ----------------------------- | --- | --- | --- | --- |
| `DecisionSequenceHead`        |     | ✅  |    |     |
| `DecisionSequenceNode`        |     | ✅  |    |     |
| `ActionSet`                   |     | ✅  |    |     |
| `ExecutionSequenceHead`       | ✅  |     | ✅ |  ✅  |
| `ExecutionSequenceNode`       | ✅  |     | ✅ |  ✅  |
