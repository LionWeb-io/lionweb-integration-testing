import {assertEquals} from "./deps.ts"

import {deserializeLanguages} from "lionweb-core"
import {LionWebJsonChunk} from "lionweb-json"
import {sortedSerializationChunk, readFileAsJson} from "lionweb-utilities"

import {fromRoot, pathOfSerialization} from "./config.ts"


Deno.test("M3 instances (Deno)", async (tctx) => {

    const javaSerializationPath = fromRoot(pathOfSerialization("m3", "Java"));
    const javaSerialization = sortedSerializationChunk(await readFileAsJson(javaSerializationPath) as LionWebJsonChunk)
    const tsSerializationPath = fromRoot(pathOfSerialization("m3", "TypeScript"));
    const tsSerialization = sortedSerializationChunk(await readFileAsJson(tsSerializationPath) as LionWebJsonChunk)
    const specSerializationPath = fromRoot(pathOfSerialization("spec", "specification"));
    const specSerialization = sortedSerializationChunk(await readFileAsJson(specSerializationPath) as LionWebJsonChunk)

    await tctx.step("check whether Java serialization of LionCore/M3 deserializes in TypeScript impl. (no assertions)", async () => {
        /* const deserializationJava = */ deserializeLanguages(javaSerialization)
    })

    const red = (text: string) => `\x1b[1;31m${text}\x1b[0m`
    const green = (text: string) => `\x1b[1;32m${text}\x1b[0m`

    await tctx.step(`check whether Java (=${red("Actual")}/left, path=${javaSerializationPath}) and TypeScript (=${green("Expected")}/right, path=${tsSerializationPath}) serialization of LionCore/M3 match`, async () => {
        assertEquals(javaSerialization, tsSerialization)
    })

    await tctx.step(`check whether Java (=${red("Actual")}/left, path=${javaSerializationPath}) serialization of LionCore/M3 matches with the specification (=${green("Expected")}/right), path=${specSerializationPath}`, async () => {
        assertEquals(javaSerialization, specSerialization)
    })

    await tctx.step(`check whether TypeScript (=${red("Actual")}/left, path=${tsSerializationPath}) serialization of LionCore/M3 matches with the specification (=${green("Expected")}/right, path=${specSerializationPath})`, async () => {
        assertEquals(tsSerialization, specSerialization)
    })

})

