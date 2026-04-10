// Copyright 2026 TRUMPF Laser SE and other contributors
//
// Licensed under the Apache License, Version 2.0 (the "License")
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
// SPDX-FileCopyrightText: 2026 TRUMPF Laser SE and other contributors
// SPDX-License-Identifier: Apache-2.0


const { validator} = require("@exodus/schemasafe")
const { readdirSync, readFileSync } = require("fs")
const { join } = require("path")

const readFileAsJson = (path) => JSON.parse(readFileSync(path).toString())

const lsFilesInPath = (fileNameEnding, path) =>
    readdirSync(path, { recursive: true })
        .filter((subPath) => subPath.endsWith(fileNameEnding))
        .map((subPath) => join(path, subPath))

const lsFilesInPaths = (fileNameEnding, paths) =>
    paths.flatMap((path) => lsFilesInPath(fileNameEnding, path))


const validateJsonInPaths = (schemaPath, fileNameEnding, paths) => {
    const schema = readFileAsJson(schemaPath)
    const validate = validator(schema, {
        includeErrors: true,
        allErrors: false,
            // Note: cardinalby/schema-validator-action@v3 configures allErrors=true, but I want to avoid too much output per JSON file.
            // Use validate-specific-message-json.js for more detailed output.
        weakFormats: true,
        extraFormats: true
    })
    let nFilesWithErrors = 0
    const jsonFilePaths = lsFilesInPaths(fileNameEnding, paths)
    for (const jsonPath of jsonFilePaths) {
        const json = readFileAsJson(jsonPath)
        const isValid = validate(json)
        if (!isValid) {
            nFilesWithErrors++
            console.error(`JSON file with path "${jsonPath}" doesn’t validate:`)
            console.dir(validate.errors)
        }
    }
    const filesTerm = `file${nFilesWithErrors === 1 ? "" : "s"}`
    if (nFilesWithErrors > 0) {
        console.error(`${nFilesWithErrors} JSON ${filesTerm} didn’t validate!`)
    } else {
        console.log(`(all ${jsonFilePaths.length} JSON ${filesTerm} validated)`)
    }
}


console.log(`validating delta JSON files against delta JSON schema:`)
validateJsonInPaths("delta.schema.json", ".delta.json", ["delta"])
console.log()

console.log(`validating serialization chunks against serialization JSON schema:`)
validateJsonInPaths(
    "serialization.schema.json",
    ".json",
    [
        "testchanges/data",
        "testset/withoutLanguage/valid",
        "testset/withLanguage/valid"
    ]
)
console.log()

