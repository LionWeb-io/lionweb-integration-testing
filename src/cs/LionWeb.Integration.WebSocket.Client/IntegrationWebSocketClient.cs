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
using LionWeb.Core.M3;
using LionWeb.Integration.Languages.Generated.V2023_1.TestLanguage.M2;
using LionWeb.Protocol.Delta.Client;
using LionWeb.WebSocket;

namespace LionWeb.Integration.WebSocket.Client;

public class IntegrationWebSocketClient
{
    private static readonly IVersion2023_1 _lionWebVersion = LionWebVersions.v2023_1;

    private static readonly List<Language> _languages =
        [TestLanguageLanguage.Instance, _lionWebVersion.BuiltIns, _lionWebVersion.LionCore];

    public static async Task Main(string[] args)
    {
        Trace.Listeners.Add(new ConsoleTraceListener());

        string name = args[0];
        string serverIp = args[1];
        int serverPort = int.Parse(args[2]);
        var tasks = args[4].Split(",").Select(s => Enum.Parse<Tasks>(s)).ToList();
        var repositoryId = "myRepo";

        Log($"Starting client {name} to connect to {serverIp}:{serverPort}@{repositoryId}");
        Log($"{name}: tasks: {string.Join(",", tasks)}");

        var forest = new Forest();

        var webSocketClient = new WebSocketClient(name, _lionWebVersion);
        
        var lionWeb = new LionWebTestClient(_lionWebVersion, _languages, $"client_{name}", forest,
            webSocketClient.Connector);

        await webSocketClient.ConnectToServer(serverIp, serverPort);

        foreach (var task in tasks)
        {
            TestPartition partition = forest.Partitions.FirstOrDefault() as TestPartition;
            switch (task)
            {
                case Tasks.SignOn:
                    await lionWeb.SignOn(repositoryId);
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.SubscribeToChangingPartitions:
                    await lionWeb.SubscribeToChangingPartitions(true, true, true);
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.SignOff:
                    await lionWeb.SignOff();
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.Wait:
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.AddPartition:
                    forest.AddPartitions([new TestPartition("partition")
                    {
                        Data = new DataTypeTestConcept("data"),
                        Links =
                        [
                            new LinkTestConcept("link")
                        ]
                    }]);
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.AddStringValue_0_1:
                    partition.Data.StringValue_0_1 = "new property";
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.SetStringValue_0_1:
                    partition.Data.StringValue_0_1 = "changed property";
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.DeleteStringValue_0_1:
                    partition.Data.StringValue_0_1 = null;
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.AddName_Containment_0_1:
                    partition.Links[0].Containment_0_1.Name = "my name";
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.AddAnnotation:
                    partition.AddAnnotations([new TestAnnotation("annotation")]);
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.AddAnnotations:
                    partition.AddAnnotations([new TestAnnotation("annotation0"), new TestAnnotation("annotation1")]);
                    lionWeb.WaitForReceived(2);
                    break;
                case Tasks.AddAnnotation_to_Containment_0_1:
                    partition.Links[0].Containment_0_1!.AddAnnotations([new TestAnnotation("annotation")]);
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.DeleteAnnotation:
                    partition.RemoveAnnotations(partition.GetAnnotations());
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.MoveAnnotationInSameParent:
                    partition.InsertAnnotations(0, [partition.GetAnnotations()[^1]]);
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.MoveAnnotationFromOtherParent:
                    partition.AddAnnotations(partition.Links[0].Containment_0_1!.GetAnnotations());
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.AddReference_0_1_to_Containment_0_1:
                    partition.Links[0].Reference_0_1 = partition.Links[0].Containment_0_1;
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.AddReference_0_1_to_Containment_1:
                    partition.Links[0].Reference_0_1 = partition.Links[0].Containment_1;
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.DeleteReference_0_1:
                    partition.Links[0].Reference_0_1 = null;
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.AddContainment_0_1:
                    partition.Links[0].Containment_0_1 = new LinkTestConcept("containment_0_1");
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.AddContainment_1:
                    partition.Links[0].Containment_1 = new LinkTestConcept("containment_1");
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.ReplaceContainment_0_1:
                    partition.Links[0].Containment_0_1!.ReplaceWith(new LinkTestConcept("substitute"));
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.DeleteContainment_0_1:
                    partition.Links[0].Containment_0_1 = null;
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.AddContainment_0_1_Containment_0_1:
                    partition.Links[0].Containment_0_1!.Containment_0_1 = new LinkTestConcept("containment_0_1_containment_0_1");
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.AddContainment_1_Containment_0_1:
                    partition.Links[0].Containment_1.Containment_0_1 = new LinkTestConcept("containment_1_containment_0_1");
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.AddContainment_0_n:
                    partition.Links[0].AddContainment_0_n([
                        new LinkTestConcept("containment_0_n_child0"), new LinkTestConcept("containment_0_n_child1")
                    ]);
                    lionWeb.WaitForReceived(2);
                    break;
                case Tasks.AddContainment_0_n_Containment_0_n:
                    partition.Links[0].AddContainment_0_n([
                        new LinkTestConcept("containment_0_n_child0")
                        {
                            Containment_0_n = [new LinkTestConcept("containment_0_n_containment_0_n_child0")]
                        }
                    ]);
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.AddContainment_1_n:
                    partition.Links[0].AddContainment_1_n([
                        new LinkTestConcept("containment_1_n_child0"), new LinkTestConcept("containment_1_n_child1")
                    ]);
                    lionWeb.WaitForReceived(2);
                    break;
                case Tasks.MoveAndReplaceChildFromOtherContainment_Single:
                    partition.Links[0].Containment_1.Containment_0_1!.ReplaceWith(partition.Links[0].Containment_0_1!.Containment_0_1!);
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.MoveAndReplaceChildFromOtherContainmentInSameParent_Single:
                    partition.Links[0].Containment_1.ReplaceWith(partition.Links[0].Containment_0_1!);
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.MoveAndReplaceChildFromOtherContainment_Multiple:
                    partition.Links[0].Containment_1_n[^1].ReplaceWith(partition.Links[0].Containment_0_n[^1].Containment_0_n[^1]);
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.MoveChildInSameContainment:
                    partition.Links[0].InsertContainment_0_n(0, [partition.Links[0].Containment_0_n[^1]]);
                    // Note: this is effectively a move rather than an insert — hence the name of the task.
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.MoveChildFromOtherContainment_Single:
                    partition.Links[0].Containment_1 = partition.Links[0].Containment_0_1!.Containment_0_1!;
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.MoveChildFromOtherContainment_Multiple:
                    partition.Links[0].InsertContainment_1_n(1, [partition.Links[0].Containment_0_n[^1].Containment_0_n[0]]);
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.MoveChildFromOtherContainmentInSameParent_Single:
                    partition.Links[0].Containment_1 = partition.Links[0].Containment_0_1!;
                    lionWeb.WaitForReceived(1);
                    break;
                case Tasks.MoveChildFromOtherContainmentInSameParent_Multiple:
                    partition.Links[0].InsertContainment_1_n(1, [partition.Links[0].Containment_0_n[^1]]);
                    lionWeb.WaitForReceived(1);
                    break;
                default:
                    throw new ArgumentException($"Can't execute task {task}");
            }
        }

        Console.ReadLine();
    }

    private static void Log(string message, bool header = false) =>
        Console.WriteLine(header
            ? $"{ILionWebClient.HeaderColor_Start}{message}{ILionWebClient.HeaderColor_End}"
            : message);
}