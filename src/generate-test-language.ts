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


// configure a convenient factory for producing a Language:
const keyAndIdMapper: StringsMapper = (...names: string[]) =>
    names.length === 1
        ? names[0]
        : names.slice(1).join("-")
const factory = new LanguageFactory("TestLanguage", "0", keyAndIdMapper, keyAndIdMapper)


// generate an enumeration with the given name, and literals named `literal1`, `literal2`, and `literal3`:
const generateTestEnumeration = (name: string): Enumeration => {
    const enumeration = factory.enumeration(name)
    enumeration.havingLiterals(...(
        [1, 2, 3].map((n) => factory.enumerationLiteral(enumeration, `literal${n}`))
    ))
    return enumeration
}
// generate two test enumerations:
const TestEnumeration = generateTestEnumeration("TestEnumeration")
generateTestEnumeration("SecondTestEnumeration")


// generate a `DataTypeTestConcept` concept with boolean-, integer-, string-, and TestEnumeration-typed properties, both required and optional:
const {booleanDataType, integerDataType, stringDataType} = builtinPrimitives
const DataTypeTestConcept = factory.concept("DataTypeTestConcept", false)
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


// generate a `LinkTestConcept` concept with containments and references in all cardinalities:
const LinkTestConcept = factory.concept("LinkTestConcept", false).implementing(builtinClassifiers.inamed)
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


// generate a test annotation:
const TestAnnotation = factory.annotation("TestAnnotation").annotating(builtinClassifiers.node).implementing(builtinClassifiers.inamed)
factory.reference(TestAnnotation, "ref").ofType(builtinClassifiers.node)

// generate a test partition:
const TestPartition = factory.concept("TestPartition", false).implementing(builtinClassifiers.inamed).isPartition()
factory.containment(TestPartition, "links").ofType(LinkTestConcept).isOptional().isMultiple()
factory.containment(TestPartition, "data").ofType(DataTypeTestConcept).isOptional()


const testLanguage = factory.language

const languagesPath = "src/languages"
const jsonAsText = (json: unknown) => JSON.stringify(json, null, 4)


// persist a textualization and a PlantUML graph of the language:
await Deno.writeTextFile(`${languagesPath}/testLanguage.txt`, languageAsText(testLanguage))
await Deno.writeTextFile(`${languagesPath}/testLanguage.puml`, generatePlantUmlForLanguage(testLanguage))


// serialize in 2023.1 format (and persist):
const serializedTestLanguage = serializeLanguages(testLanguage)
await Deno.writeTextFile(`${languagesPath}/testLanguage.2023.1.json`, jsonAsText(serializedTestLanguage))

// function to set all "version" fields in the given serialization chunk to the specified version:
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


// modify version for 2024.1 version, modify reference objects to comply with the specification, and persist:
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


// modify version for 2025.1 version, and persist:
setVersion(serializedTestLanguage, "2025.1")
// Note: reference objects were already modified to comply with the specification in the previous step!
await Deno.writeTextFile(`${languagesPath}/testLanguage.2025.1.json`, jsonAsText(serializedTestLanguage))

