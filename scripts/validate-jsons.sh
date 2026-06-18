#!/bin/sh -e

./scripts/download-delta.schema.json.sh
./scripts/download-serialization.schema.json.sh
cd node
npm i
./validate-all-jsons.ts
cd ..

