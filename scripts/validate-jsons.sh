#!/bin/sh -e

./scripts/download-json-schemas.sh

cd ts
npm i
./scripts/validate-all-jsons.ts
cd ..

