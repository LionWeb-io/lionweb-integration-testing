#!/bin/sh -e

./scripts/download-json-schemas.sh
cd node
npm i
./src/validate-all-jsons.ts
cd ..

