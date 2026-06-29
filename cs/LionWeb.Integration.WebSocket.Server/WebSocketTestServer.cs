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

using LionWeb.Core;
using LionWeb.Protocol.Delta.Repository;
using LionWeb.WebSocket;

namespace LionWeb.Integration.WebSocket.Server;

public class WebSocketTestServer : WebSocketServer
{
    private readonly Action<string> _logger;

    public WebSocketTestServer(LionWebVersions lionWebVersion, string ipAddress, int port, Action<string> logger) : base(lionWebVersion)
    {
        _logger = logger;
        StartServer(ipAddress, port);
        _logger($"Server started on port {port}.");
    }

    protected override void Log(string message, bool header = false)
    {
        _logger(header
            ? $"{ILionWebRepository.HeaderColor_Start}{message}{ILionWebRepository.HeaderColor_End}"
            : message);
    }
}