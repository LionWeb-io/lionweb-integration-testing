# README

This .NET solution holds the prototypal implementation of the WebSocket-driven client and repository implementations of the LionWeb delta protocol.

## Installation/requirements

The solution relies on the presence of _three_ repositories, right next to each other, so:

* `lionweb-integration-testing`: this repository.
* `lionweb-csharp`: the C# implementation of LionWeb, minus a delta protocol implementation.
* `lionweb-typescript`: the TypeScript implementation of LionWeb, including the prototypal delta protocol implementation which is (only) present in the `deltas/develop` branch of that repository.

“Right next to each other” means that these repositories should reside inside one directory.
The source code in the various projects relies on the C# implementation.
The integration tests – in the `LionWeb.Integration.WebSocket.Tests` project – also relies on the TypeScript implementation.

Make sure to check out the `deltas/develop` branch – or a branch branched off from that one – of the `lionweb-typescript` repository, to be able to run the integration tests in the `WebSocketServerTests` class which exercise the server using either C# or TS clients (or both).

