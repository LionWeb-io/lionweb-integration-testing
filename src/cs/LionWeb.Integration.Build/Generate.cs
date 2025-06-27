// Copyright 2024 TRUMPF Laser SE and other contributors
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
// SPDX-FileCopyrightText: 2024 TRUMPF Laser SE and other contributors
// SPDX-License-Identifier: Apache-2.0

using LionWeb.Core;
using LionWeb.Core.M2;
using LionWeb.Core.M3;
using LionWeb.Core.Serialization;
using LionWeb.Generator;
using LionWeb.Generator.Names;

foreach (LionWebVersions lionWebVersion in LionWebVersions.AllPureVersions)
{
    Console.WriteLine($"\n### LionWeb specification version: {lionWebVersion}\n");
    
    var shapesLanguage = DeserializeExternalLanguage(lionWebVersion, "shapes").First();

    var lionWebVersionDirectory = "V" + lionWebVersion.VersionString.Replace('.', '_');
    string prefix = $"LionWeb.Integration.Languages.Generated.{lionWebVersionDirectory}";
    List<Names> names =
    [
        new(shapesLanguage, $"{prefix}.Shapes.M2")
    ];

    if (lionWebVersion.LionCore is ILionCoreLanguageWithStructuredDataType)
    {
        var structureNameLanguage = DeserializeExternalLanguage(lionWebVersion, "structureName").First();
        names.Add(new(structureNameLanguage, $"{prefix}.StructureName.M2"));
    }
    
    var generationPath = $"../../../../LionWeb.Integration.Languages/Generated/{lionWebVersionDirectory}";
    Directory.CreateDirectory(generationPath);

    foreach (var name in names)
    {
        var generator = new GeneratorFacade { Names = name, LionWebVersion = lionWebVersion};
        generator.Generate();
        Console.WriteLine($"generated code for: {name.Language.Name}");

        var path = @$"{generationPath}/{name.Language.Name}.g.cs";
        generator.Persist(path);
        Console.WriteLine($"persisted to: {path}");
    }
}

return;

DynamicLanguage[] DeserializeExternalLanguage(LionWebVersions lionWebVersion, string name, params Language[] dependentLanguages)
{
    SerializationChunk serializationChunk = JsonUtils.ReadJsonFromString<SerializationChunk>(File.ReadAllText($"../../../../../languages/{name}.{lionWebVersion.VersionString}.json"));
    return new LanguageDeserializer(lionWebVersion)
    {
        StoreUncompressedIds = true
    }.Deserialize(serializationChunk, dependentLanguages).ToArray();
}
