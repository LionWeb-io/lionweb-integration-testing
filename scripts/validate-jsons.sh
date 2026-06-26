#!/bin/sh -e

./scripts/download-json-schemas.sh
cd ts
npm i
./src/validate-all-jsons.ts
cd ..

