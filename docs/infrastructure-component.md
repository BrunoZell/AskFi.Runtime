# Infrastructure Component

This subsystem takes care of data persistence and messaging.

All persistent or transmitted data in the system is _content addressable_. Which esentially is just to identify a given piece of data by its hash. For this, the Runtime adheres to the specs of [IPFS](https://docs.ipfs.tech/) and [IPLD](https://ipld.io/docs/).
