const { validator} = require("@exodus/schemasafe")
const { readdirSync, readFileSync } = require("fs")
const { join } = require("path")

const readFileAsJson = (path) => JSON.parse(readFileSync(path).toString())

const lsFilesInPath = (fileNameEnding, path) =>
    readdirSync(path, { recursive: true })
        .filter((subPath) => subPath.endsWith(fileNameEnding))
        .map((subPath) => join(path, subPath))

const lsFilesInPaths = (fileNameEnding, paths) =>
    paths.flatMap((path) => lsFilesInPath(fileNameEnding, path))


const validateJsonInPaths = (schemaPath, fileNameEnding, paths) => {
    const schema = readFileAsJson(schemaPath)
    const validate = validator(schema, { includeErrors: true })
    let nFilesWithErrors = 0
    const jsonFilePaths = lsFilesInPaths(fileNameEnding, paths)
    for (const jsonPath of jsonFilePaths) {
        const json = readFileAsJson(jsonPath)
        const isValid = validate(json)
        if (!isValid) {
            nFilesWithErrors++
            console.error(`JSON file with path "${jsonPath}" doesn’t validate:`)
            console.dir(validate.errors)
        }
    }
    if (nFilesWithErrors > 0) {
        console.error(`${nFilesWithErrors} JSON files didn’t validate!`)
    } else {
        console.log(`(all ${jsonFilePaths.length} JSON files validated)`)
    }
}


console.log(`validating delta JSON files against delta JSON schema:`)
validateJsonInPaths("delta.schema.json", ".delta.json", ["delta"])
console.log()

console.log(`validating serialization chunks against serialization JSON schema:`)
validateJsonInPaths(
    "serialization.schema.json",
    ".json",
    [
        "testchanges/data",
        "testset/withoutLanguage/valid",
        "testset/withLanguage/valid"
    ]
)
console.log()

