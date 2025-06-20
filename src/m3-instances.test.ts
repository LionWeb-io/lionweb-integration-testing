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

    await tctx.step("check whether Java and TypeScript serializations match", async () => {
        const serializationJava = await readFileAsJson(fromRoot(pathOfSerialization("m3", "Java"))) as LionWebJsonChunk
        const serializationTypeScript = await readFileAsJson(fromRoot(pathOfSerialization("m3", "TypeScript"))) as LionWebJsonChunk
        assertEquals(sortedSerializationChunk(serializationJava), sortedSerializationChunk(serializationTypeScript))
    })

})

