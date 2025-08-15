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
using LionWeb.Core.M3;
using LionWeb.Core.Notification;
using LionWeb.Integration.Languages.Generated.V2023_1.Shapes.M2;
using LionWeb.Integration.Languages.Generated.V2023_1.TestLanguage.M2;
using LionWeb.Protocol.Delta;
using LionWeb.Protocol.Delta.Client;
using LionWeb.Protocol.Delta.Client.Forest;
using LionWeb.Protocol.Delta.Client.Partition;
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
        var tasks = args[4].Split(",").Select(s => Enum.Parse<Tasks>(s)).ToList();

        Log($"Starting client {name} to connect to {serverIp}:{serverPort}");
        Log($"{name}: tasks: {string.Join(",", tasks)}");

        var webSocketClient = new WebSocketClient(name);
        var partition = webSocketClient.GetPartition(partitionType);
        var lionWeb = new LionWebTestClient(_lionWebVersion, _languages, $"client_{name}", partition, webSocketClient);
        var forest = new Forest();
        var lionWeb = new LionWebTestClient(_lionWebVersion, _languages, $"client_{name}", forest, webSocketClient);

        if (!tasks.Contains(Tasks.Partition))
            forest.AddPartitions([partition]);

        await webSocketClient.ConnectToServer(serverIp, serverPort);

        foreach (var task in tasks)
        {
            switch (task)
            {
                case Tasks.SignOn:
                    await webSocketClient.SignOn(lionWeb);
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.SignOff:
                    await webSocketClient.SignOff(lionWeb);
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.Wait:
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.AddDocs:
                    ((Geometry)partition).Documentation = new Documentation("documentation");
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.SetDocsText:
                    ((Geometry)partition).Documentation!.Text = "hello there";
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.AddStringValue_0_1:
                    ((DataTypeTestConcept)partition).StringValue_0_1 = "new property";
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.SetStringValue_0_1:
                    ((DataTypeTestConcept)partition).StringValue_0_1 = "changed property";
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.DeleteStringValue_0_1:
                    ((DataTypeTestConcept)partition).StringValue_0_1 = null;
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.AddAnnotation:
                    ((LinkTestConcept)partition).AddAnnotations([new TestAnnotation("annotation")]);
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.AddAnnotations:
                    ((LinkTestConcept)partition).AddAnnotations([new TestAnnotation("annotation0"), new TestAnnotation("annotation1")]);
                    lionWeb.WaitForReceived(2);
                    break;
                case Tasks.AddAnnotation_to_Containment_0_1:
                    ((LinkTestConcept)partition).Containment_0_1!.AddAnnotations([new TestAnnotation("annotation")]);
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.DeleteAnnotation:
                    ((LinkTestConcept)partition).RemoveAnnotations(((LinkTestConcept)partition).GetAnnotations());
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.MoveAnnotationInSameParent:
                    ((LinkTestConcept)partition).InsertAnnotations(0, [((LinkTestConcept)partition).GetAnnotations()[^1]]);
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.MoveAnnotationFromOtherParent:
                    ((LinkTestConcept)partition).AddAnnotations(((LinkTestConcept)partition).Containment_0_1!.GetAnnotations());
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.AddReference_0_1_to_Containment_0_1:
                    ((LinkTestConcept)partition).Reference_0_1 = ((LinkTestConcept)partition).Containment_0_1;
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.AddReference_0_1_to_Containment_1:
                    ((LinkTestConcept)partition).Reference_0_1 = ((LinkTestConcept)partition).Containment_1;
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.DeleteReference_0_1:
                    ((LinkTestConcept)partition).Reference_0_1 = null;
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.AddContainment_0_1:
                    ((LinkTestConcept)partition).Containment_0_1 = new LinkTestConcept("containment_0_1");
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.AddContainment_1:
                    ((LinkTestConcept)partition).Containment_1 = new LinkTestConcept("containment_1");
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.ReplaceContainment_0_1:
                    ((LinkTestConcept)partition).Containment_0_1!.ReplaceWith(new LinkTestConcept("substitute"));
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.DeleteContainment_0_1:
                    ((LinkTestConcept)partition).Containment_0_1 = null;
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.AddContainment_0_1_Containment_0_1:
                    ((LinkTestConcept)partition).Containment_0_1!.Containment_0_1 = new LinkTestConcept("containment_0_1_containment_0_1");
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.AddContainment_1_Containment_0_1:
                    ((LinkTestConcept)partition).Containment_1.Containment_0_1 = new LinkTestConcept("containment_1_containment_0_1");
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.AddContainment_0_n:
                    ((LinkTestConcept)partition).AddContainment_0_n([new LinkTestConcept("containment_0_n_child0"), new LinkTestConcept("containment_0_n_child1")]);
                    lionWeb.WaitForReceived(2);
                    break;
                case Tasks.AddContainment_0_n_Containment_0_n:
                    ((LinkTestConcept)partition).AddContainment_0_n([new LinkTestConcept("containment_0_n_child0") {Containment_0_n = [new LinkTestConcept("containment_0_n_containment_0_n_child0")] }]);
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.AddContainment_1_n:
                    ((LinkTestConcept)partition).AddContainment_1_n([new LinkTestConcept("containment_1_n_child0"), new LinkTestConcept("containment_1_n_child1")]);
                    lionWeb.WaitForReceived(2);
                    break;
                case Tasks.MoveAndReplaceChildFromOtherContainment_Single:
                    ((LinkTestConcept)partition).Containment_1.Containment_0_1!.ReplaceWith(((LinkTestConcept)partition).Containment_0_1!.Containment_0_1!);
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.MoveAndReplaceChildFromOtherContainmentInSameParent_Single:
                    ((LinkTestConcept)partition).Containment_1.ReplaceWith(((LinkTestConcept)partition).Containment_0_1!);
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.MoveAndReplaceChildFromOtherContainment_Multiple:
                    ((LinkTestConcept)partition).Containment_1_n[^1].ReplaceWith(((LinkTestConcept)partition).Containment_0_n[^1].Containment_0_n[^1]);
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.MoveChildInSameContainment:
                    ((LinkTestConcept)partition).InsertContainment_0_n(0, [((LinkTestConcept)partition).Containment_0_n[^1]]);
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.MoveChildFromOtherContainment_Single:
                    ((LinkTestConcept)partition).Containment_1 = ((LinkTestConcept)partition).Containment_0_1!.Containment_0_1!;
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.MoveChildFromOtherContainment_Multiple:
                    ((LinkTestConcept)partition).InsertContainment_1_n(1,[((LinkTestConcept)partition).Containment_0_n[^1].Containment_0_n[0]]);
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.MoveChildFromOtherContainmentInSameParent_Single:
                    ((LinkTestConcept)partition).Containment_1 = ((LinkTestConcept)partition).Containment_0_1!;
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.Partition:
                    forest.AddPartitions([new LinkTestConcept("partition")]);
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.MoveChildFromOtherContainmentInSameParent_Multiple:
                    ((LinkTestConcept)partition).InsertContainment_1_n(1,[((LinkTestConcept)partition).Containment_0_n[^1]]);
                    lionWeb.WaitForReceived(1);
                    break;
            }
        }
        
        Console.ReadLine();
    }

    private IPartitionInstance GetPartition(string partitionType)
    {
        return partitionType switch
        {
            "LinkTestConcept" => new LinkTestConcept("a"),
            "DataTypeTestConcept" => new DataTypeTestConcept("a"),
            "Geometry" => new Geometry("a"),
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
    public IDeltaContent Convert(INotification notification)
        => _mapper.Map(notification);

    private static void Log(string message, bool header = false) =>
        Console.WriteLine(header
            ? $"{ILionWebClient.HeaderColor_Start}{message}{ILionWebClient.HeaderColor_End}"
            : message);
}