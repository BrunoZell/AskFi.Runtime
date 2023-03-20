# AskFi.Runtime.DataModel

Most data models used to reference all data flowing through are trees. Such trees, especially when strongly typed, are easiest to model in an algebraic type system.

This library is introduced as a place for such type definitions in F#.

Those types may later be exposed to tracing and persistence-code. This library could then be used as the _interface library_ for all runtime-produced data.
