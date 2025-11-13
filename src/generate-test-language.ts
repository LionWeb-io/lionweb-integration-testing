import {
    builtinClassifiers,
    builtinPrimitives,
    Enumeration,
    LanguageFactory,
    Link,
    serializeLanguages
} from "lionweb-core"
import { LionWebJsonChunk } from "lionweb-json"
import { StringsMapper } from "lionweb-ts-utils"
import { generatePlantUmlForLanguage, languageAsText } from "lionweb-utilities"

const keyAndIdMapper: StringsMapper = (...names: string[]) =>
    names.length === 1
        ? names[0]
        : names.slice(1).join("-")

const factory = new LanguageFactory("TestLanguage", "0", keyAndIdMapper, keyAndIdMapper)

const generateTestEnumeration = (name: string): Enumeration => {
    const enumeration = factory.enumeration(name)
    enumeration.havingLiterals(...(
        [1, 2, 3].map((n) => factory.enumerationLiteral(enumeration, `literal${n}`))
    ))
    return enumeration
}
const TestEnumeration = generateTestEnumeration("TestEnumeration")
generateTestEnumeration("SecondTestEnumeration")

const {booleanDataType, integerDataType, stringDataType} = builtinPrimitives
const DataTypeTestConcept = factory.concept("DataTypeTestConcept", false).isPartition()
;[false, true].forEach((optional) => {
    ;[["boolean", booleanDataType], ["integer", integerDataType], ["string", stringDataType], ["enum", TestEnumeration]].forEach(
        ([typeName, dataType]) => {
            const property = factory.property(DataTypeTestConcept, `${typeName}Value_${optional ? "0_" : ""}1`).ofType(dataType)
            if (optional) {
                property.isOptional()
            }
        }
    )
})

const LinkTestConcept = factory.concept("LinkTestConcept", false).implementing(builtinClassifiers.inamed).isPartition()
type LinkType = "containment" | "reference"
const linkTypes: LinkType[] = ["containment", "reference"]
linkTypes.forEach((linkType) => {
    ;[false, true].forEach((multiple) => {
        ;[true, false].forEach((optional) => {
            const cardinalityString = (!optional && !multiple)
                ? "1"
                : `${optional ? "0" : "1"}_${multiple ? "n" : "1"}`
            const link = (factory[linkType](LinkTestConcept, `${linkType}_${cardinalityString}`) as Link).ofType(LinkTestConcept)
            if (optional) {
                link.isOptional()
            }
            if (multiple) {
                link.isMultiple()
            }
        })
    })
})

factory.annotation("TestAnnotation").annotating(builtinClassifiers.node)

const testLanguage = factory.language

const languagesPath = "src/languages"
await Deno.writeTextFile(`${languagesPath}/testLanguage.txt`, languageAsText(testLanguage))
await Deno.writeTextFile(`${languagesPath}/testLanguage.puml`, generatePlantUmlForLanguage(testLanguage))
const serializedTestLanguage = serializeLanguages(testLanguage)

const jsonAsText = (json: unknown) => JSON.stringify(json, null, 2)
await Deno.writeTextFile(`${languagesPath}/testLanguage.2023.1.json`, jsonAsText(serializedTestLanguage))

const setVersion = (chunkJson: LionWebJsonChunk, version: string) => {
    chunkJson.serializationFormatVersion = version
    chunkJson.languages.forEach((usedLanguage) => { usedLanguage.version = version })
    chunkJson.nodes.forEach((node) => {
        node.classifier.version = version
        node.properties.forEach(({ property }) => { property.version = version })
        node.containments.forEach(({ containment }) => { containment.version = version })
        node.references.forEach(({ reference }) => { reference.version = version })
    })
}

setVersion(serializedTestLanguage, "2024.1")

const oldPrefix = "LionCore-builtins-"
serializedTestLanguage.nodes.forEach(({ references }) => {
    references.forEach(({ targets }) => {
        targets.forEach((target) => {
            if (target.reference.startsWith(oldPrefix)) {
                target.resolveInfo = `LionWeb.LionCore_builtins.${target.reference.substring(oldPrefix.length)}`
                target.reference = null
            }
        })
    })
})

await Deno.writeTextFile(`${languagesPath}/testLanguage.2024.1.json`, jsonAsText(serializedTestLanguage))

setVersion(serializedTestLanguage, "2025.1")
await Deno.writeTextFile(`${languagesPath}/testLanguage.2025.1.json`, jsonAsText(serializedTestLanguage))

