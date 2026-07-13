# CLI delta test tools

## Building

After having run `npm i[nstall]`, the CLI test client and repository are built as follows:

```shell
$ npm run build:client
```

or

```shell
$ tsc
```

Run the linting as follows:

```shell
$ npm run lint:client
```


## CLI client

The CLI client can then be started as follows:

```shell
$ node dist/cli-client.js <port> <clientID> [tasks]
```

The arguments are as follows:

- `<port>`: The port of the WebSocket where the LionWeb delta protocol repository is running on `localhost`.
- `<clientID>`: The ID that the client identifies itself with at the repository.
- `[tasks]` (optional — the rest of the arguments are required): a comma-separated list of tasks.
  Run `node dist/cli-client.js` with less 4 arguments to have the help text shown with the recognized task names.

The `--protocol-log==<path>` option ensures that the client logs all messages exchanged with the repository to a file with the given path.

***Note*** that it's assumed that the initial (states of the models) on (any) client(s) and repository are identical!


## CLI Repository

The CLI repository can be started (as a Node.js program) as follows:

```shell
$ node dist/cli-repository.js <port>
```

The one (required) argument is `<port>`: the number of the WebSocket port where the LionWeb delta protocol repository is going to run.

