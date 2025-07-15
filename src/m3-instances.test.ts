import {assertEquals} from "./deps.ts"

import {create as ansiColors} from "ansi-colors"
const {green, red} = ansiColors()

import {deserializeLanguages, lioncoreBuiltins} from "lionweb-core"
import {LionWebJsonChunk} from "lionweb-json"
import {sortedSerializationChunk, readFileAsJson} from "lionweb-utilities"

import {fromRoot, pathOfSerialization} from "./config.ts"


Deno.test("M3 instances (Deno)", async (tctx) => {

    const withBuiltins = (serializationChunk: LionWebJsonChunk, origin?: string): LionWebJsonChunk => {
        const {languages} = serializationChunk
        if (!languages.some(({key}) => key === lioncoreBuiltins.key)) {
            languages.push({ key: lioncoreBuiltins.key, version: lioncoreBuiltins.version })
            console.log(`added LionCore-builtins (version: ${lioncoreBuiltins.version}) to used languages of serialization chunk${origin ? ` of origin "${origin}"` : ""}`)
        }
        return serializationChunk
    }

    const javaSerializationPath = fromRoot(pathOfSerialization("m3", "Java"));
    const javaSerialization = sortedSerializationChunk(await readFileAsJson(javaSerializationPath) as LionWebJsonChunk)
    const tsSerializationPath = fromRoot(pathOfSerialization("m3", "TypeScript"));
    const tsSerialization = sortedSerializationChunk(await readFileAsJson(tsSerializationPath) as LionWebJsonChunk)
    const specSerializationPath = fromRoot(pathOfSerialization("spec", "specification"));
    const specSerialization = withBuiltins(sortedSerializationChunk(await readFileAsJson(specSerializationPath) as LionWebJsonChunk), "spec")

    await tctx.step("check whether Java serialization of LionCore/M3 deserializes in TypeScript impl. (no assertions)", () => {
        /* const deserializationJava = */ deserializeLanguages(javaSerialization)
    })

    await tctx.step(`check whether Java (=${red("Actual")}/left, path=${javaSerializationPath}) and TypeScript (=${green("Expected")}/right, path=${tsSerializationPath}) serialization of LionCore/M3 match`, () => {
        assertEquals(javaSerialization, tsSerialization)
    })

    await tctx.step(`check whether Java (=${red("Actual")}/left, path=${javaSerializationPath}) serialization of LionCore/M3 matches with the specification (=${green("Expected")}/right), path=${specSerializationPath}`, () => {
        assertEquals(javaSerialization, specSerialization)
    })

    await tctx.step(`check whether TypeScript (=${red("Actual")}/left, path=${tsSerializationPath}) serialization of LionCore/M3 matches with the specification (=${green("Expected")}/right, path=${specSerializationPath})`, () => {
        assertEquals(tsSerialization, specSerialization)
    })

})

