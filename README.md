# LionWeb integration testing

Automated tests that check that the other repos within the [LionWeb GitHub (organization)](https://github.com/LionWeb-io) integrate well and consistently with each other.


## The build

The definition of “well” in the previous sentences is not very broad — effectively it means: the build runs green.
With “build” we mean the [GitHub Action](https://github.com/LionWeb-io/lionweb-integration-testing/actions/workflows/build.yaml), from here on.

Note that this repository serves as a **“canary in a coal mine”**: if its build turns red, it means there’s a mismatch(/incompatibility) between either an implementation and the specification (and/or between implementations).
That also means that modifications and additions to this repository – in the form of PRs – might turn the build red when run on the PR’s branch.
That is a good thing: it means that the modification/addition spots an incompatibility that wasn’t spotted before.

Whenever the build of this repository fails, the steps to take are as follows:

1. find out what the incompatibility is,
2. find out where it originates from — typically an implementation, sometimes the specification,
3. submit an issue on the corresponding repository — which is _most likely not_ this repository,
4. fix that issue (there),
5. trigger the build here to verify it runs green again.

The build – in the form of a GitHub Action name “LionWeb integration tests” – does the following:

* validate the delta payloads under [`delta/`](delta) against the [Delta JSON Schema](https://raw.githubusercontent.com/LionWeb-io/specification/refs/heads/main/delta/delta.schema.json) for those,
* validate the _valid_ serialization chunks (recognizable as such by the presence of a `valid` fragment in their paths) under [`testchanges/`](testchanges) and [`testset/`](testset) against the [Serialization chunk JSON Schema](https://raw.githubusercontent.com/LionWeb-io/specification/refs/heads/main/serialization/serialization.schema.json) for those,
* run the integration test suite implemented in TypeScript, by running `scripts/run-ts-tests.sh`.
  For this suite to run, a number of the other repositories have to be cloned, by running `scripts/clone-repos.sh`.

The automated tests in the cloned repositories are **not** run.

And remember: a green build is not a guarantee for the absence of bugs (or presence of quality), but a red build is definitely a guarantee that you need to look at something.

Note also that the build sometimes fails during the “Test all C# projects” part, with something like the following in the output:

```text
  Error Message:
   System.Net.HttpListenerException : Address already in use
  Stack Trace:
     at System.Net.HttpEndPointManager.GetEPListener(String host, Int32 port, HttpListener listener, Boolean secure)
   at System.Net.HttpEndPointManager.AddPrefixInternal(String p, HttpListener listener)
   at System.Net.HttpEndPointManager.AddListener(HttpListener listener)
   at System.Net.HttpListener.Start()
   at LionWeb.WebSocket.WebSocketServer.StartServer(String ipAddress, Int32 port)
```

If this happens, then just rerun the job: usually(/often), the job succeeds the next time.
*Why* this happens is unclear to us, as we do take pains to assign each C# WebSocket server its own, unique port number.

### JSON validation

The schema validation that the build performs can’t be run locally directly, but the following should be equivalent:

```shell
$ ./scripts/validate-jsons.sh
```

The `validate-specific-message-json.ts` script can be used to troubleshoot message JSONs that don’t validate.

```shell
$ ./scripts/download-json-schemas.sh   # (download full JSON Schema for delta protocol messages)

$ ./ts/src/validate-specific-message-json.ts
Usage: execute
    node ts/src/validate-specific-message-json.ts <path to JSON with message> [message kind]
to validate that JSON as a message of the indicated kind — hopefully producing more understandable errors.
If the message kind is not given, we try to derive that from the file name, although that might fail.
In addition, a JSON schema that only pertains to that message kind is saved to a file with path "schemas/<message kind>.specific-schema.json".

$ ./ts/src/validate-specific-message-json.ts delta/event/ErrorEvent.delta.json
Derived message kind from path of JSON file as: ErrorEvent
JSON file with path "delta/event/ErrorEvent.delta.json" contains a valid message of kind ErrorEvent.
```

(All JSON files within the `schemas/` dir. are Git-ignored.)


## Installation requirements

* [Node.js](https://nodejs.org/en/download) (including NPM), currently (at least) version 24.y.z — for running Node.js scripts such as `ts/src/validate-all-jsons.ts`.
* Java 11 for the [`lionweb-jvm` repo](repos/lionweb-jvm).


## Test language

The [`testLanguage`](testLanguage) directory contains (artifacts relating to) the `TestLanguage` language.

By running `generate-test-language.sh`, the `TestLanguage` language is:

* generated and serialized, for LionWeb versions [`2023.1`](testLanguage/testLanguage.2023.1.json), [`2024.1`](testLanguage/testLanguage.2024.1.json), and [`2026.1`](testLanguage/testLanguage.2026.1.json),
* [textualized](testLanguage/testLanguage.txt), and
* [rendered as PlantUML diagram](testLanguage/testLanguage.puml),
* generated as a C# code base.


## Test data for serialization format validators

The [`testset/`](testset) directory contains test data that can be used to test validators that validate whether serialization chunks (JSON files) conform to the serialization format specification.
The subdirectory [`withoutLanguage/`](testset/withoutLanguage) pertains to serialization chunks that are to be validated *without* a registered language.
The subdirectory [`withLanguage/`](testset/withLanguage) pertains to serialization chunks that are to be validated *against* a registered language — m.n. the one serialized as [`myLang.language.json`](testset/withLanguage/myLang.language.json).
Either subdirectories contain two subdirectories named `valid` and `invalid`.
Each (nested) subdirectory under `invalid` contains a `__TestExpectation.json` file which specifies which chunk fails validation, and if so, with which error type.
The format of the `__TestExpectation.json` files is as follows:

```json lines
{
  "errors": [
    {
      "file": "<file next to __TestExpectation.json>",
      "error": "<error type>"
    },
    ...
  ]
}
```

Consider the following example of a member of the `errors` array:

```json
{
  "file": "empty.json",
  "error": "PropertyValueIncorrect"
}
```

This means that validating the `empty.json` file is expected to produce an error of type `PropertyValueIncorrect` (and that it’s the first one produced).

