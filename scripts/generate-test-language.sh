#!/bin/sh -e

./ts/src/generate-test-language.ts

cd cs/LionWeb.Integration.Build
dotnet run Generate.cs
cd ../..

