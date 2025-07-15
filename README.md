# LionWeb integration testing

Automated tests that check that the other repos within the [LionWeb GitHub](https://github.com/LionWeb-io) integrate well and consistently with each other.
The definition of “well” in the previous sentences is not very broad — effectively it means: the build runs green.

Note that this repository serves as a **“canary in a coal mine”**: if its build turns red, it means there's a mismatch(/incompatibility) between either an implementation and the specification (and/or between implementations).
That also means that modifications and additions to this repository – in the form of PRs – might turn the build red when run on the PR's branch.
That is a good thing: it means that the modification/addition spots an incompatibility that wasn't spotted before.

Whenever the build of this repository fails, the steps to take are as follows:

1. find out what the incompatibility is,
2. find out where it originates from — typically an implementation, sometimes the specification,
3. submit an issue on the corresponding repository — which is _most likely not_ this repository,
4. fix that issue (there),
5. trigger the build here to verify it runs green again.

The build – in the form of a GitHub Action name “LionWeb integration tests” – does the following:

* validate the delta payloads under [`delta/`](./delta) against the [Delta JSON Schema](https://raw.githubusercontent.com/LionWeb-io/specification/refs/heads/main/delta/delta.schema.json) for those,
* validate the _valid_ serialization chunks (recognizable as such by the presence of a `valid` fragment in their paths) under [`testchanges/`](./testchanges) and [`testset/`](./testset) against the [Serialization chunk JSON Schema](https://raw.githubusercontent.com/LionWeb-io/specification/refs/heads/main/serialization/serialization.schema.json) for those,
* run the integration test suite which is implemented using Deno, by running `./test.sh`.
  For this suite to run, (most of) the other repositories are cloned, by running `./clone.sh`.

The automated tests in the cloned repositories are **not** run.

And remember: a green build is not a guarantee for the absence of bugs (or presence of quality), but a red build is definitely a guarantee that you need to look at something.


## Installation requirements

* [Deno](https://deno.com), currently (at least) version 2.4.1 - for the integration tests written in Deno-compliant TypeScript in [`src/`](./src).
  (Deno is used instead of Node.js because Deno can reliably execute TypeScript code natively.)
* Java 11 (but really Java 8) - for the [`lioncore-java` repo](./repos/lioncore-java).

