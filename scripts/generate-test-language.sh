#!/bin/sh -e

cd ts
npm i
./src/generate-test-language.ts
cd ..

cd cs/LionWeb.Integration.Build
dotnet run Generate.cs
cd ../..

