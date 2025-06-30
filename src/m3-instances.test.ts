import {assertEquals} from "./deps.ts"

import {deserializeLanguages} from "lionweb-core"
import {LionWebJsonChunk} from "lionweb-json"
import {sortedSerializationChunk, readFileAsJson} from "lionweb-utilities"

import {fromRoot, pathOfSerialization} from "./config.ts"


Deno.test("M3 instances (Deno)", async (tctx) => {

    await tctx.step("check whether Java serialization of LionCore/M3 deserializes in TypeScript impl. (no assertions)", async () => {
        const serializationJava = await readFileAsJson(fromRoot(pathOfSerialization("m3", "Java")))
        /* const deserializationJava = */ deserializeLanguages(serializationJava as LionWebJsonChunk)
    })

    const red = (text: string) => `\x1b[1;31m${text}\x1b[0m`
    const green = (text: string) => `\x1b[1;32m${text}\x1b[0m`

    await tctx.step(`check whether Java (=${red("Actual")}/left) and (=${green("Expected")}/right) serialization of LionCore/M3 match`, async () => {
        const serializationJava = await readFileAsJson(fromRoot(pathOfSerialization("m3", "Java"))) as LionWebJsonChunk
        const serializationTypeScript = await readFileAsJson(fromRoot(pathOfSerialization("m3", "TypeScript"))) as LionWebJsonChunk
        assertEquals(sortedSerializationChunk(serializationJava), sortedSerializationChunk(serializationTypeScript))
    })

    await tctx.step(`check whether Java (=${red("Actual")}/left) serialization of LionCore/M3 matches with the specification (=${green("Expected")}/right)`, async () => {
        const serializationJava = await readFileAsJson(fromRoot(pathOfSerialization("m3", "Java"))) as LionWebJsonChunk
        const serializationSpecification = await readFileAsJson(fromRoot(pathOfSerialization("spec", "specification"))) as LionWebJsonChunk
        assertEquals(sortedSerializationChunk(serializationJava), sortedSerializationChunk(serializationSpecification))
    })

    await tctx.step(`check whether TypeScript (=${red("Actual")}/left) serialization of LionCore/M3 matches with the specification (=${green("Expected")}/right)`, async () => {
        const serializationTypeScript = await readFileAsJson(fromRoot(pathOfSerialization("m3", "TypeScript"))) as LionWebJsonChunk
        const serializationSpecification = await readFileAsJson(fromRoot(pathOfSerialization("spec", "specification"))) as LionWebJsonChunk
        assertEquals(sortedSerializationChunk(serializationTypeScript), sortedSerializationChunk(serializationSpecification))
    })

})

