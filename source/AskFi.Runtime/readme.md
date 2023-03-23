# AskFi Runtime Internals

`AskbotBuilder` is the type to instantiate to confgiure an `Askbot` with. After configuration is completed (which is, adding _Observers_, _Brokers_, and a _Strategy_), `AskbotBuilder.Build` can be called to produce an `Askbot`.

On the Askbot instance, `Askbot.Run` can be called to start the instance. When called, the Askbot is first initialized:

- For each _Observer_, a related `ObservationSequencer` is created.
- A `WorldSequener` is created, where all outputs from the `ObserveationSequencer` are correlated and sequenced
- A `SessionController` is created, which:
    - executes the _Strategy_ every time an _Observation_ is preceived
    - in case the _Strategy_ outputs a decision to initiate one or more _Actions_, those intents are routed to the according _Brokers_ (if configured)
