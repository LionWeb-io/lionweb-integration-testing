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
using LionWeb.Core.M1.Event.Partition;
using LionWeb.Core.M3;
using LionWeb.Core.Serialization;
using LionWeb.Core.Serialization.Delta;
using LionWeb.Core.Serialization.Delta.Command;
using LionWeb.Core.Serialization.Delta.Event;
using LionWeb.Core.Serialization.Delta.Query;

namespace LionWeb.Integration.Languages;

using ParticipationId = NodeId;

public interface IDeltaRepositoryConnector
{
    Task Send(IClientInfo clientInfo, IDeltaContent content);
    Task SendAll(IDeltaContent content);
    event EventHandler<IDeltaMessageContext> Receive;
}

public interface IDeltaMessageContext
{
    IClientInfo ClientInfo { get; }
    IDeltaContent Content { get; }
}

public interface IClientInfo
{
    ParticipationId ParticipationId { get; }
}

public class LionWebServer
{
    private readonly string _name;
    private readonly IDeltaRepositoryConnector _connector;
    private readonly DeltaProtocolPartitionCommandReceiver _commandReceiver;
    private readonly PartitionEventToDeltaEventMapper _mapper;

    private long _messageCount;
    public long MessageCount => Interlocked.Read(ref _messageCount);

    public LionWebServer(LionWebVersions lionWebVersion, List<Language> languages, string name,
        IPartitionInstance partition, IDeltaRepositoryConnector connector)
    {
        _name = name;
        _connector = connector;
        _mapper = new PartitionEventToDeltaEventMapper(new ExceptionParticipationIdProvider(), new EventSequenceNumberProvider(), lionWebVersion);
        
        Dictionary<string, IReadableNode> sharedNodeMap = [];
        var partitionEventHandler = new PartitionEventHandler(name);
        DeserializerBuilder deserializerBuilder = new DeserializerBuilder()
                .WithLionWebVersion(lionWebVersion)
                .WithLanguages(languages)
                .WithHandler(new ReceiverDeserializerHandler())
            ;
        Dictionary<CompressedMetaPointer, IKeyed>
            sharedKeyedMap = DeltaCommandToDeltaEventMapper.BuildSharedKeyMap(languages);
        _commandReceiver = new DeltaProtocolPartitionCommandReceiver(
            partitionEventHandler,
            sharedNodeMap,
            sharedKeyedMap,
            deserializerBuilder
        );
        var replicator = new RewritePartitionEventReplicator(partition, sharedNodeMap);
        replicator.ReplicateFrom(partitionEventHandler);

        var publisher = replicator;
        publisher.Subscribe<IPartitionEvent>(SendPartitionEventToAllClients);

        connector.Receive += (_, content) => Receive(content);
    }

    private void SendPartitionEventToAllClients(object? sender, IPartitionEvent? partitionEvent)
    {
        if (partitionEvent == null)
            return;

        IDeltaEvent deltaEvent = _mapper.Map(partitionEvent);
        SendAll(deltaEvent);
    }

    private async Task SendAll(IDeltaContent deltaContent)
    {
        switch (deltaContent)
        {
            case IDeltaEvent deltaEvent:
                var commandSource = deltaEvent is { OriginCommands: { } cmds }
                    ? cmds.First()
                    : null;
                Debug.WriteLine(
                    $"{_name}: sending event: {deltaEvent.GetType()}({commandSource},{deltaEvent.SequenceNumber})");
                break;

            default:
                Debug.WriteLine($"{_name}: sending: {deltaContent.GetType()}");
                break;
        }

        await _connector.SendAll(deltaContent);
    }

    private async Task Send(IClientInfo clientInfo, IDeltaContent deltaContent)
    {
        Debug.WriteLine($"{_name}: sending to {clientInfo}: {deltaContent.GetType()}");
        await _connector.Send(clientInfo, deltaContent);
    }

    private async Task Receive(IDeltaMessageContext messageContext)
    {
        try
        {
            var content = messageContext.Content;
            content.InternalParticipationId = messageContext.ClientInfo.ParticipationId;
            Debug.WriteLine(
                $"{_name}: received {content.GetType().Name} for {messageContext.ClientInfo.ParticipationId}: {content})");
            Interlocked.Increment(ref _messageCount);

            switch (content)
            {
                case IDeltaCommand command:
                    Debug.WriteLine($"{_name}: received command: {command.GetType()}({command.CommandId})");
                    _commandReceiver.Receive(command);
                    break;

                case SignOnRequest signOnRequest:
                    Debug.WriteLine(
                        $"{_name}: received {nameof(SignOnRequest)} for {messageContext.ClientInfo}: {signOnRequest})");
                    await Send(messageContext.ClientInfo,
                        new SignOnResponse(messageContext.ClientInfo.ParticipationId, signOnRequest.QueryId, null));
                    break;

                default:
                    Debug.WriteLine($"{_name}: ignoring received: {content.GetType()}({content.InternalParticipationId})");
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }
}

public class LionWebTestServer(
    LionWebVersions lionWebVersion,
    List<Language> languages,
    string name,
    IPartitionInstance partition,
    IDeltaRepositoryConnector connector)
    : LionWebServer(lionWebVersion, languages, name, partition, connector)
{
    public int WaitCount { get; private set; }

    private const int SleepInterval = 100;

    private void WaitForCount(int count)
    {
        while (MessageCount < count)
        {
            Thread.Sleep(SleepInterval);
        }
    }

    public void WaitForReceived(int delta) =>
        WaitForCount(WaitCount += delta);
}

public class ExceptionParticipationIdProvider : IParticipationIdProvider
{
    public string ParticipationId => throw new NotImplementedException();
}

internal class ReceiverDeserializerHandler : DeserializerExceptionHandler
{
    public override bool SkipDeserializingDependentNode(CompressedId id)
    {
        return false;
    }
}

public class EventSequenceNumberProvider : IEventSequenceNumberProvider
{
    private long next = 0;
    public long Create() => ++next;
}