#!/bin/sh -e

CONFIGURATION=${1:-Release}
TEST_PROJECT=LionWeb.Integration.WebSocket.Tests

cd cs
echo "Building (from clean) and running WebSocket tests with configuration=$CONFIGURATION."
dotnet clean
dotnet build --configuration $CONFIGURATION "./$TEST_PROJECT/"
mkdir -p "$TEST_PROJECT/logs"
dotnet test --configuration $CONFIGURATION --logger trx "./$TEST_PROJECT/" > "$TEST_PROJECT/logs/test-$CONFIGURATION.log"
cd ..

