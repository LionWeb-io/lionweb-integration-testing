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
using LionWeb.Core.M3;
using LionWeb.Core.Utilities;
using LionWeb.Integration.Languages.Generated.V2023_1.TestLanguage.M2;
using NUnit.Framework.Legacy;

namespace LionWeb.Integration.WebSocket.Tests;

internal class IdenticalIdComparer : Comparer
{
    public IdenticalIdComparer(IList<INode?> left, IList<INode?> right) : base(left, right)
    {
    }

    protected override List<IDifference> CompareNode(IReadableNode? leftOwner, IReadableNode? left, Link? containment, IReadableNode? rightOwner,
        IReadableNode? right)
    {
        var result = base.CompareNode(leftOwner, left, containment, rightOwner, right);
        if (left is not null && right is not null && left.GetId() != right.GetId())
        {
            result.Insert(0, new NodeIdDifference(left, right));
        }
        return result;
    }
}

public record NodeIdDifference(IReadableNode Left, IReadableNode Right) : DifferenceBase
{
    protected override string Describe() => 
        $"Node id: {LeftDescription()}: {Left.GetId()} {NC(Left)} vs. {RightDescription()}: {Right.GetId()} {NC(Right)}";
}

public class IdenticalIdComparerTests
{
    [Test]
    public void Same()
    {
        var differences = new IdenticalIdComparer([new LinkTestConcept("a")], [new LinkTestConcept("a")]).Compare();
        ClassicAssert.IsEmpty(differences);
    }
    
    [Test]
    public void Different()
    {
        var differences = new IdenticalIdComparer([new LinkTestConcept("a")], [new LinkTestConcept("b")]).Compare();
        ClassicAssert.IsNotEmpty(differences);
    }
}