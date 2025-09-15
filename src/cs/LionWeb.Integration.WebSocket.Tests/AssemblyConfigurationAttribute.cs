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

namespace LionWeb.Integration.WebSocket.Tests;

/// Makes build-time configuration values available at run-time.
/// Use with <c>&lt;AssemblyAttribute&gt;</c> in .csproj file.
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
sealed class AssemblyConfigurationAttribute(string key, string value) : Attribute
{
    public string Key { get; } = key;
    public string Value { get; } = value;

    public override string ToString()
    {
        return $"{nameof(Key)}: {Key}, {nameof(Value)}: {Value}";
    }

    public static string Get(string key) => typeof(AssemblyConfigurationAttribute)
        .Assembly
        .GetCustomAttributes(typeof(AssemblyConfigurationAttribute), false)
        .OfType<AssemblyConfigurationAttribute>()
        .FirstOrDefault(cfg => cfg.Key == key)
        ?.Value ?? throw new ArgumentException($"Missing configuration attribute {key}");
}