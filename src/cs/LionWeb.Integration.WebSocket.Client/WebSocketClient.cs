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
using System.Net.WebSockets;
using System.Text;
using LionWeb.Core;
using LionWeb.Core.M1;
using LionWeb.Core.M1.Event;
using LionWeb.Core.M3;
using LionWeb.Integration.Languages.Generated.V2023_1.Shapes.M2;
using LionWeb.Integration.Languages.Generated.V2023_1.TestLanguage.M2;
using LionWeb.Protocol.Delta;
using LionWeb.Protocol.Delta.Client;
using LionWeb.Protocol.Delta.Message;

namespace LionWeb.Integration.WebSocket.Client;

public class WebSocketClient(string name) : IDeltaClientConnector
{
    public const int BUFFER_SIZE = 0x10000;

    public const string ClientStartedMessage = "Client started.";

    private static readonly IVersion2023_1 _lionWebVersion = LionWebVersions.v2023_1;

    private static readonly List<Language> _languages =
        [TestLanguageLanguage.Instance, ShapesLanguage.Instance, _lionWebVersion.BuiltIns, _lionWebVersion.LionCore];

    public static async Task Main(string[] args)
    {
        Trace.Listeners.Add(new ConsoleTraceListener());

        string name = args[0];
        string serverIp = args[1];
        int serverPort = int.Parse(args[2]);
        string partitionType = args[3];
        var tasks = args[4].Split(",");

        Log($"Starting client {name} to connect to {serverIp}:{serverPort}");
        Log($"{name}: tasks: {string.Join(",", tasks)}");

        var webSocketClient = new WebSocketClient(name);
        var partition = webSocketClient.GetPartition(partitionType);
        var lionWeb = new LionWebTestClient(_lionWebVersion, _languages, $"client_{name}", partition, webSocketClient);

        await webSocketClient.ConnectToServer(serverIp, serverPort);

        foreach (var task in tasks)
        {
            switch (task)
            {
                case "SignOn":
                    await webSocketClient.SignOn(lionWeb);
                    break;
                case "SignOff":
                    await webSocketClient.SignOff(lionWeb);
                    break;
                case "Wait":
                    lionWeb.WaitForReplies(1);
                    break;
                case "AddDocs":
                    ((Geometry)partition).Documentation = new Documentation("documentation");
                    lionWeb.WaitForReplies(1);
                    break;
                case "SetDocsText":
                    ((Geometry)partition).Documentation!.Text = "hello there";
                    lionWeb.WaitForReplies(1);
                    break;
                case "AddStringValue_0_1":
                    ((DataTypeTestConcept)partition).StringValue_0_1 = "new property";
                    lionWeb.WaitForReplies(1);
                    break;
                case "SetStringValue_0_1":
                    ((DataTypeTestConcept)partition).StringValue_0_1 = "changed property";
                    lionWeb.WaitForReplies(1);
                    break;
                case "DeleteStringValue_0_1":
                    ((DataTypeTestConcept)partition).StringValue_0_1 = null;
                    lionWeb.WaitForReplies(1);
                    break;
                case "AddAnnotation":
                    ((LinkTestConcept)partition).AddAnnotations([new TestAnnotation("annotation")]);
                    lionWeb.WaitForReplies(1);
                    break;
                case "AddAnnotations":
                    ((LinkTestConcept)partition).AddAnnotations([new TestAnnotation("annotation0"), new TestAnnotation("annotation1")]);
                    lionWeb.WaitForReplies(2);
                    break;
                case "AddAnnotation_to_Containment_0_1":
                    ((LinkTestConcept)partition).Containment_0_1!.AddAnnotations([new TestAnnotation("annotation")]);
                    lionWeb.WaitForReplies(1);
                    break;
                case "DeleteAnnotation":
                    ((LinkTestConcept)partition).RemoveAnnotations(((LinkTestConcept)partition).GetAnnotations());
                    lionWeb.WaitForReplies(1);
                    break;
                case "MoveAnnotationInSameParent":
                    ((LinkTestConcept)partition).InsertAnnotations(0, [((LinkTestConcept)partition).GetAnnotations()[^1]]);
                    lionWeb.WaitForReplies(1);
                    break;
                case "MoveAnnotationFromOtherParent":
                    ((LinkTestConcept)partition).AddAnnotations(((LinkTestConcept)partition).Containment_0_1!.GetAnnotations());
                    lionWeb.WaitForReplies(1);
                    break;
                case "AddContainment_0_1":
                    ((LinkTestConcept)partition).Containment_0_1 = new LinkTestConcept("containment_0_1");
                    lionWeb.WaitForReplies(1);
                    break;
                case "AddContainment_1":
                    ((LinkTestConcept)partition).Containment_1 = new LinkTestConcept("containment_1");
                    lionWeb.WaitForReplies(1);
                    break;
                case "ReplaceContainment_0_1":
                    ((LinkTestConcept)partition).Containment_0_1 = new LinkTestConcept("substitute");
                    lionWeb.WaitForReplies(1);
                    break;
                case "DeleteContainment_0_1":
                    ((LinkTestConcept)partition).Containment_0_1 = null;
                    lionWeb.WaitForReplies(1);
                    break;
                case "AddContainment_0_1_Containment_0_1":
                    ((LinkTestConcept)partition).Containment_0_1!.Containment_0_1 = new LinkTestConcept("containment_0_1_containment_0_1");
                    lionWeb.WaitForReplies(1);
                    break;
                case "AddContainment_1_Containment_0_1":
                    ((LinkTestConcept)partition).Containment_1.Containment_0_1 = new LinkTestConcept("containment_1_containment_0_1");
                    lionWeb.WaitForReplies(1);
                    break;
                case "MoveAndReplaceChildFromOtherContainment_Single":
                    ((LinkTestConcept)partition).Containment_1.Containment_0_1 = ((LinkTestConcept)partition).Containment_0_1!.Containment_0_1!;
                    lionWeb.WaitForReplies(1);
                    break;
                case "AddContainment_0_n":
                    ((LinkTestConcept)partition).AddContainment_0_n([new LinkTestConcept("containment_0_n_child0"), new LinkTestConcept("containment_0_n_child1")]);
                    lionWeb.WaitForReplies(2);
                    break;
                case "AddContainment_1_n":
                    ((LinkTestConcept)partition).AddContainment_1_n([new LinkTestConcept("containment_1_n_child0"), new LinkTestConcept("containment_1_n_child1")]);
                    lionWeb.WaitForReplies(2);
                    break;
                case "MoveAndReplaceChildFromOtherContainment_Multiple":
                    ((LinkTestConcept)partition).Containment_1_n[^1].ReplaceWith(((LinkTestConcept)partition).Containment_0_n[^1]);
                    lionWeb.WaitForReplies(1);
                    break;
                case "MoveChildInSameContainment":
                    ((LinkTestConcept)partition).InsertContainment_0_n(0, [((LinkTestConcept)partition).Containment_0_n[^1]]);
                    lionWeb.WaitForReplies(1);
                    break;
                case "MoveChildFromOtherContainment_Single":
                    ((LinkTestConcept)partition).Containment_1 = ((LinkTestConcept)partition).Containment_0_1!.Containment_0_1!;
                    lionWeb.WaitForReplies(1);
                    break;
                case "MoveChildFromOtherContainmentInSameParent":
                    ((LinkTestConcept)partition).Containment_1 = ((LinkTestConcept)partition).Containment_0_1!;
                    lionWeb.WaitForReplies(1);
                    break;
            }
        }
        
        Console.ReadLine();
    }

    private IPartitionInstance GetPartition(string partitionType)
    {
        return partitionType switch
        {
            "LionWeb.Integration.Languages.Generated.V2023_1.TestLanguage.M2.LinkTestConcept" => new LinkTestConcept("a"),
            "LionWeb.Integration.Languages.Generated.V2023_1.TestLanguage.M2.DataTypeTestConcept" => new DataTypeTestConcept("a"),
            "LionWeb.Integration.Languages.Generated.V2023_1.Shapes.M2.Geometry" => new Geometry("a"),
            _ =>  throw new ArgumentException("Invalid partition type specified.")
        };
    }
    private readonly EventToDeltaCommandMapper _mapper =
        new(new PartitionEventToDeltaCommandMapper(new CommandIdProvider(), _lionWebVersion));

    private async Task SignOn(LionWebTestClient lionWeb) => 
        await lionWeb.SignOn();


    private async Task SignOff(LionWebTestClient lionWeb) => 
        await lionWeb.SignOff();

    private int nextQueryId = 0;

    private string QueryId() =>
        $"{name}-{nextQueryId++}";

    private readonly DeltaSerializer _deltaSerializer = new();
    private readonly ClientWebSocket _clientWebSocket = new ClientWebSocket();

    /// <inheritdoc />
    public event EventHandler<IDeltaContent>? ReceiveFromRepository;

    public async Task ConnectToServer(string ipAddress, int port) =>
        await ConnectToServer($"ws://{ipAddress}:{port}");

    public async Task ConnectToServer(string serverUri)
    {
        await _clientWebSocket.ConnectAsync(new Uri(serverUri), CancellationToken.None);

        Log($"{name}: {ClientStartedMessage} Connected to the server: {serverUri}");

        Task.Run(async () =>
        {
            // Receive messages from the server
            byte[] receiveBuffer = new byte[BUFFER_SIZE];
            while (_clientWebSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result =
                    await _clientWebSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string receivedMessage = Encoding.UTF8.GetString(receiveBuffer, 0, result.Count);
                    // Log($"XXClient: received message: {receivedMessage}");
                    var deserialized = _deltaSerializer.Deserialize<IDeltaContent>(receivedMessage);
                    // do NOT await
                    Task.Run(() => ReceiveFromRepository?.Invoke(this, deserialized));
                    // Log($"XXClient: processed message: {receivedMessage}");
                }
            }
        });
    }

    /// <inheritdoc />
    public async Task SendToRepository(IDeltaContent content) =>
        await Send(_deltaSerializer.Serialize(content));

    public async Task Send(string msg) =>
        await _clientWebSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg)), WebSocketMessageType.Text,
            true, CancellationToken.None);

    /// <inheritdoc />
    public IDeltaContent Convert(IEvent internalEvent)
        => _mapper.Map(internalEvent);

    private static void Log(string message, bool header = false) =>
        Console.WriteLine(header
            ? $"{ILionWebClient.HeaderColor_Start}{message}{ILionWebClient.HeaderColor_End}"
            : message);
}