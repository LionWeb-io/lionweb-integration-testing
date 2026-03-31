const { readFileSync, writeFileSync } = require("fs")
const { argv, exit } = require("process")
const { validator } = require("@exodus/schemasafe")

if (argv.length < 4) {
    console.log(`Usage: execute`)
    console.log(`\tnode validate-specific-message-json.js <path to JSON with message> <message kind>`)
    console.log(`to validate that JSON as a message of the indicated kind — hopefully producing more understandable errors.`)
    console.log(`In addition, a JSON schema that only pertains to that message kind is saved to a file with name "<message kind>.schema.json".`)
    exit(64)
}

const messagePath = argv[2]
const messageKind = argv[3]

const readFileAsJson = (path) => JSON.parse(readFileSync(path).toString())

const schema = readFileAsJson("delta.schema.json")

const messageSchema = schema.$defs[messageKind]

if (messageSchema === undefined) {
    console.error(`"${messageKind}" is not a valid/known message kind — exiting.`)
    exit(64)
}

const referredDefs = []
const gatherReferredDefsFrom = (thing) => {
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
    }
}

gatherReferredDefsFrom(messageSchema)

const effectiveSchema = {
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

writeFileSync(`${messageKind}.schema.json`, JSON.stringify(effectiveSchema, null, 2))

const validate = validator(effectiveSchema, {
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

