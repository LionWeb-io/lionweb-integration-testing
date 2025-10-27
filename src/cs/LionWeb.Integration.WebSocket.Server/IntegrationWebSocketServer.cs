// Copyright 2025 LionWeb Project
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
// SPDX-FileCopyrightText: 2025 LionWeb Project
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using LionWeb.Core;
using LionWeb.Core.M1;
using LionWeb.Core.M2;
using LionWeb.Core.M3;
using LionWeb.Core.Notification;
using LionWeb.Integration.Languages.Generated.V2023_1.TestLanguage.M2;
using LionWeb.Protocol.Delta.Repository;
using LionWeb.WebSocket;

namespace LionWeb.Integration.WebSocket.Server;

public class IntegrationWebSocketServer
{
    private static string IpAddress { get; set; } = "localhost";

    public static void Main(string[] args)
    {
        Trace.Listeners.Add(new ConsoleTraceListener());

        Log($"server args: {string.Join(", ", args)}");

        var port = args.Length > 0
            ? int.Parse(args[0])
            : 40000;

        LionWebVersions lionWebVersion = LionWebVersions.v2023_1;
        List<Language> languages =
            [TestLanguageLanguage.Instance, lionWebVersion.BuiltIns, lionWebVersion.LionCore];

        var webSocketServer = new WebSocketServer(lionWebVersion)
        {
            Languages = languages
        };

        webSocketServer.StartServer(IpAddress, port);

        IPartitionInstance serverPartition = languages
            .SelectMany(l => l.Entities)
            .OfType<Concept>()
            .Where(c => c.Partition)
            .Select(c => (IPartitionInstance)c.GetLanguage().GetFactory().CreateNode("a", c))
            .First();

        var serverForest = new Forest();
        Log($"Server partition: <{serverPartition.GetClassifier().Name}>{serverPartition.PrintIdentity()}");

        var lionWebServer = new LionWebRepository(lionWebVersion, webSocketServer.Languages, "server",
            serverForest,
            webSocketServer.Connector);

        Console.ReadLine();
        webSocketServer.Stop();
    }

    public required List<Language> Languages { get; init; }


    private static void Log(string message, bool header = false) =>
        Console.WriteLine(header
            ? $"{ILionWebRepository.HeaderColor_Start}{message}{ILionWebRepository.HeaderColor_End}"
            : message);
}