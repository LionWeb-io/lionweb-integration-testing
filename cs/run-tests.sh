#!/bin/sh -e

CONFIGURATION=${1:-Release}
TEST_PROJECT=LionWeb.Integration.WebSocket.Tests

echo "Building (from clean) and running WebSocket tests with configuration=$CONFIGURATION."
dotnet clean
dotnet build --configuration $CONFIGURATION "./$TEST_PROJECT/"
dotnet test --configuration $CONFIGURATION --logger trx "./$TEST_PROJECT/" > "$TEST_PROJECT/logs/test-$CONFIGURATION.log"
