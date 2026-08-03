#/bin/bash

pglite="npx @electric-sql/pglite-socket --port=45432 --host=0.0.0.0 --debug=0"
lwserver="npx @lionweb/server-server@0.4.2-beta.1 --run --config server-config-pglite.json"

$pglite &
P1=$!

sleep 2

$lwserver &
P2=$!

wait $P1 $P2