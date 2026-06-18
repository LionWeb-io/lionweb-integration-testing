#!/usr/bin/env node --no-warnings


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


import { readFileSync, writeFileSync } from "node:fs"
import { argv, exit } from "node:process"
import { validator } from "@exodus/schemasafe"


// process arguments:

if (argv.length < 3) {
    console.log(
`Usage: execute
    node validate-specific-message-json.js <path to JSON with message> [message kind]
to validate that JSON as a message of the indicated kind — hopefully producing more understandable errors.
If the message kind is not given, we try to derive that from the file name, although that might fail.
In addition, a JSON schema that only pertains to that message kind is saved to a file with name "<message kind>.specific-schema.json".`
    )
    exit(64)
}

type StringTransformer = (str: string) => string

const postfixTryRemover= (postfix: string): StringTransformer =>
    (str) =>
        str.endsWith(postfix) ? str.substring(0, str.length - postfix.length) : str

const composeTransforms = (...transforms: StringTransformer[]): StringTransformer =>
    (str) =>
        transforms.reduce((current, transform) => transform(current), str)

const fileNameFrom = composeTransforms(
    (path) => path.substring(path.lastIndexOf("/") + 1),
    postfixTryRemover(".delta.json"),
    postfixTryRemover(".json")
)

const messagePath = argv[2]
const optionalArg2 = argv[3]
const messageKind = optionalArg2 ?? fileNameFrom(messagePath)
if (!optionalArg2) {
    console.log(`Derived message kind from path of JSON file as: ${messageKind}`)
}


const readFileAsJson = (path: string) =>
    JSON.parse(readFileSync(path).toString())


// compute JSON Schema specific to message kind:

const schema = readFileAsJson("schemas/delta.schema.json")

const messageSchema = schema.$defs[messageKind]

if (messageSchema === undefined) {
    console.error(`"${messageKind}" is not a valid/known message kind — exiting.`)
    exit(64)
}

const referredDefs: string[] = []
const gatherReferredDefsFrom = (thing: object) => {
    if ("properties" in thing) {
        Object.values(thing.properties).forEach(gatherReferredDefsFrom)
    } else if (thing.type === "array") {
        gatherReferredDefsFrom(thing.items)
    } else if ("$ref" in thing) {
        const referredDef = thing.$ref.substring("#/$defs/".length)
        if (referredDefs.indexOf(referredDef) === -1) {
            referredDefs.push(referredDef)
            gatherReferredDefsFrom(schema.$defs[referredDef])
        }
    } else if ("anyOf" in thing) {
        thing.anyOf.forEach(gatherReferredDefsFrom)
    } else if ("oneOf" in thing) {
        thing.oneOf.forEach(gatherReferredDefsFrom)
    }
}

gatherReferredDefsFrom(messageSchema)

const specificSchema = {
    $schema: schema.$schema,
    $id: schema.$id,
    title: schema.title + ` — specialized for messageKind ${messageKind}`,
    description: schema.description,
    type: "object",
    ...messageSchema,
    $defs: Object.fromEntries(
        Object.entries(schema.$defs)
            .filter(([key, _]) => referredDefs.indexOf(key) > -1)
    )
}

writeFileSync(`schemas/${messageKind}.specific-schema.json`, JSON.stringify(specificSchema, null, 2))


// validate delta JSON:

const validate = validator(specificSchema, {
    includeErrors: true,
    allErrors: true,
    weakFormats: true,
    extraFormats: true
})

const isValid = validate(readFileAsJson(messagePath))
if (isValid) {
    console.log(`JSON file with path "${messagePath}" contains a valid message of kind ${messageKind}.`)
} else {
    console.error(`JSON file with path "${messagePath}" doesn’t validate:`)
    console.dir(validate.errors)
}

