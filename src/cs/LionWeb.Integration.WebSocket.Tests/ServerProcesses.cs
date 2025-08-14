// Copyright 2024 TRUMPF Laser GmbH
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// SPDX-FileCopyrightText: 2024 TRUMPF Laser GmbH
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using LionWeb.Integration.WebSocket.Server;

namespace LionWeb.Integration.WebSocket.Tests;

public enum ServerProcesses
{
    CSharp,
    OtherCSharp,
    LionWebServer // The server from the lionweb-server project
}

public static class ServerProcessesExtensions
{
    public static Process Create(this ServerProcesses process, int port, string additionalServerParameters,
        out string readyTrigger, out string errorTrigger) => process switch
    {
        ServerProcesses.CSharp => CSharpServer(port, additionalServerParameters, out readyTrigger,
            out errorTrigger),
        ServerProcesses.OtherCSharp => CSharpServer(port, additionalServerParameters, out readyTrigger,
            out errorTrigger),
        ServerProcesses.LionWebServer => LionWebServer(port, additionalServerParameters, out readyTrigger,
            out errorTrigger),
        _ => throw new ArgumentOutOfRangeException(nameof(process), process, null)
    };

    private static Process CSharpServer(int port, string additionalServerParameters, out string readyTrigger,
        out string errorTrigger)
    {
        TestContext.WriteLine($"AdditionalServerParameters: {additionalServerParameters}");
        var result = new Process();
        result.StartInfo.FileName = "dotnet";
        result.StartInfo.WorkingDirectory =
            $"{Directory.GetCurrentDirectory()}/../../../../LionWeb.Integration.WebSocket.Server";
        result.StartInfo.Arguments = $"""
                                      run
                                      --no-build
                                      --configuration {AssemblyConfigurationAttribute.Configuration}
                                      {port}
                                      {additionalServerParameters}
                                      """.ReplaceLineEndings(" ");
        result.StartInfo.UseShellExecute = false;
        readyTrigger = WebSocketServer.ServerStartedMessage;
        errorTrigger = "Exception";

        Console.WriteLine($"CSharpServer arguments: {result.StartInfo.Arguments}");
        
        return result;
    }

    private static Process LionWebServer(int port, string additionalServerParameters, out string readyTrigger, out string errorTrigger)
    {
        string serverConfig = $"{Directory.GetCurrentDirectory()}/../../../../../../../lionweb-integration-testing/src/cs/LionWeb.Integration.WebSocket.Tests/lionweb-server-config.json"; 
        string serverDir = $"{Directory.GetCurrentDirectory()}/../../../../../../../lionweb-server/packages/server";
        // Read config file
        JsonNode configJson = ReadJsonFromFile(serverDir + "/" + "server-config.json");
        configJson["server"]["serverPort"] = port;
        WriteJsonToFile(serverConfig, configJson);
        TestContext.WriteLine($"Config file: {serverConfig}");

        TestContext.WriteLine($"LionWebServer.AdditionalServerParameters: {additionalServerParameters}");
        var result = new Process();
        result.StartInfo.FileName = "node";
        result.StartInfo.WorkingDirectory = serverDir;
        result.StartInfo.Arguments = "./dist/server.js --run --config ../../../lionweb-integration-testing/src/cs/LionWeb.Integration.WebSocket.Tests/lionweb-server-config.json";
        result.StartInfo.UseShellExecute = false;
        readyTrigger = "Server is running";
        errorTrigger = "Error";
        return result;
    }

    private static Process LionWebServer(int port, string additionalServerParameters, out string readyTrigger, out string errorTrigger)
    {
        string currentDirFile = $"{Directory.GetCurrentDirectory()}/../../../../../../../lionweb-integration-testing/src/cs/LionWeb.Integration.WebSocket.Tests/lionweb-server-config.json"; 
        string serverDir = $"{Directory.GetCurrentDirectory()}/../../../../../../../lionweb-server/packages/server";
        // Read config file
        JsonNode configJson = ReadJsonFromFile(serverDir + "/" + "server-config.json");
        configJson["server"]["serverPort"] = port;
        WriteJsonToFile(currentDirFile, configJson);
        TestContext.WriteLine($"Config file: {currentDirFile}");

        TestContext.WriteLine($"LionWebServer.AdditionalServerParameters: {additionalServerParameters}");
        var result = new Process();
        result.StartInfo.FileName = "node";
        result.StartInfo.WorkingDirectory =
            $"{Directory.GetCurrentDirectory()}/../../../../../../../lionweb-server/packages/server";
        result.StartInfo.Arguments = "./dist/server.js --run --config ../../../lionweb-integration-testing/src/cs/LionWeb.Integration.WebSocket.Tests/lionweb-server-config.json";
        result.StartInfo.UseShellExecute = false;
        readyTrigger = "Server is running";
        errorTrigger = "Error";
        return result;
    }
    
    // Method to write data to a JSON file
    private static void WriteJsonToFile(string filePath, JsonNode node)
    {
        string json = JsonSerializer.Serialize(node, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json, Encoding.ASCII);
    }

    // Method to read data from a JSON file
    static JsonNode ReadJsonFromFile(string filePath)
    {
        string json = File.ReadAllText(filePath, Encoding.ASCII);
        JsonNode node = JsonNode.Parse(json)!;
        return node;
    }
}
