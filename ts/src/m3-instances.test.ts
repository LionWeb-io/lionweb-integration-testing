import { assert } from "chai"
export const { deepEqual } = assert

import { create as ansiColors } from "ansi-colors"
const { green, red} = ansiColors()

import { deserializeLanguages, LionWebVersions } from "@lionweb/core"
import type { LionWebJsonChunk } from "@lionweb/json"
import { readFileAsJsonSync } from "@lionweb/node-utils"
import { sortedSerializationChunk } from "@lionweb/utilities"
import { join } from "node:path"


describe("M3 instances", () => {

    const withBuiltins = (serializationChunk: LionWebJsonChunk, origin?: string): LionWebJsonChunk => {
        const {languages} = serializationChunk
        const builtins = LionWebVersions.v2023_1.builtinsFacade.language
        if (!languages.some(({key}) => key === builtins.key)) {
            languages.push({ key: builtins.key, version: builtins.version })
            console.log(`added LionCore-builtins (version: ${builtins.version}) to used languages of serialization chunk${origin ? ` of origin "${origin}"` : ""}`)
        }
        return serializationChunk
    }

    const reposPath = "../../repos"
    const pathInRepos = (path: string) => join(reposPath, path)
    const javaSerializationPath = pathInRepos("lionweb-jvm/core/src/test/resources/serialization/lioncore.json")
    const javaSerialization = sortedSerializationChunk(readFileAsJsonSync(javaSerializationPath) as LionWebJsonChunk)
    const tsSerializationPath = pathInRepos("lionweb-typescript/packages/build/artifacts/core/v2023_1/lioncore.json")
    const tsSerialization = sortedSerializationChunk(readFileAsJsonSync(tsSerializationPath) as LionWebJsonChunk)
    const specSerializationPath = pathInRepos("specification/2023.1/metametamodel/lioncore.json")
    const specSerialization = withBuiltins(sortedSerializationChunk(readFileAsJsonSync(specSerializationPath) as LionWebJsonChunk), "spec")

    it("check whether Java serialization of LionCore/M3 deserializes in TypeScript impl. (no assertions)", () => {
        /* const deserializationJava = */ deserializeLanguages(javaSerialization)
    })

    it(`check whether Java (=${red("Actual")}/left, path=${javaSerializationPath}) and TypeScript (=${green("Expected")}/right, path=${tsSerializationPath}) serialization of LionCore/M3 match`, () => {
        deepEqual(javaSerialization, tsSerialization)
    })

    it(`check whether Java (=${red("Actual")}/left, path=${javaSerializationPath}) serialization of LionCore/M3 matches with the specification (=${green("Expected")}/right), path=${specSerializationPath}`, () => {
        deepEqual(javaSerialization, specSerialization)
    })

    it(`check whether TypeScript (=${red("Actual")}/left, path=${tsSerializationPath}) serialization of LionCore/M3 matches with the specification (=${green("Expected")}/right, path=${specSerializationPath})`, () => {
        deepEqual(tsSerialization, specSerialization)
    })

})

