import { assert } from "chai"
export const { deepEqual } = assert

import { create as ansiColors } from "ansi-colors"
const { green, red} = ansiColors()

import { deserializeLanguages, LionWebVersions } from "@lionweb/core"
import type { LionWebJsonChunk } from "@lionweb/json"
import { getFromHttps } from "@lionweb/node-utils"
import { sortedSerializationChunk } from "@lionweb/utilities"


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

    const javaSerializationUrl = "https://raw.githubusercontent.com/LionWeb-io/lionweb-jvm/refs/heads/main/core/src/test/resources/serialization/lioncore.json"
    const tsSerializationUrl = "https://raw.githubusercontent.com/LionWeb-io/lionweb-typescript/refs/heads/develop/packages/build/artifacts/core/v2023_1/lioncore.json"
    const specSerializationUrl = "https://raw.githubusercontent.com/LionWeb-io/specification/refs/heads/main/2023.1/metametamodel/lioncore.json"

    const getJsonFromHttps = async (url: string) =>
        JSON.parse((await getFromHttps(url)).toString())

    let _savedChunks: Record<string, unknown>
    const chunks = async () => {
        if (_savedChunks === undefined) {
            _savedChunks = {
                javaSerialization: sortedSerializationChunk(await getJsonFromHttps(javaSerializationUrl) as LionWebJsonChunk, true),
                tsSerialization: sortedSerializationChunk(await getJsonFromHttps(tsSerializationUrl) as LionWebJsonChunk, true),
                specSerialization: withBuiltins(sortedSerializationChunk(await getJsonFromHttps(specSerializationUrl) as LionWebJsonChunk, true), "spec")
            }
        }
        return _savedChunks
    }

    it("check whether Java serialization of LionCore/M3 deserializes in TypeScript impl. (no assertions)", async () => {
        /* const deserializationJava = */ deserializeLanguages((await chunks()).javaSerialization)
    })

    it(`check whether Java (=${red("Actual")}/left, URL=${javaSerializationUrl}) and TypeScript (=${green("Expected")}/right, URL=${tsSerializationUrl}) serialization of LionCore/M3 match`, async () => {
        deepEqual((await chunks()).javaSerialization, (await chunks()).tsSerialization)
    })

    it(`check whether Java (=${red("Actual")}/left, URL=${javaSerializationUrl}) serialization of LionCore/M3 matches with the specification (=${green("Expected")}/right), URL=${specSerializationUrl}`, async () => {
        deepEqual((await chunks()).javaSerialization, (await chunks()).specSerialization)
    })

    it(`check whether TypeScript (=${red("Actual")}/left, URL=${tsSerializationUrl}) serialization of LionCore/M3 matches with the specification (=${green("Expected")}/right, URL=${specSerializationUrl})`, async () => {
        deepEqual((await chunks()).tsSerialization, (await chunks()).specSerialization)
    })

})

