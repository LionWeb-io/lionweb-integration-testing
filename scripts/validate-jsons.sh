#!/bin/sh -e

./scripts/download-json-schemas.sh

cd ts
npm i
cd ..

./ts/scripts/validate-all-jsons.ts

