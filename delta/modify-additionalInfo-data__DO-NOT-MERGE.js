const { readdirSync, readFileSync, writeFileSync } = require("fs")

const modifyDelta = (deltaJson) => {
    let modified = false
    if (deltaJson.additionalInfos) {
        deltaJson.additionalInfos.forEach((additionalInfo) => {
            if (additionalInfo.data && Array.isArray(additionalInfo.data)) {
                additionalInfo.data = Object.fromEntries(
                    additionalInfo.data.map(({key, value}) => [key, value])
                )
                modified = true
            }
        })
    }
    if (deltaJson.messageKind.startsWith("Composite") && deltaJson.parts) {
        deltaJson.parts.forEach((subDeltaJson) => {
            modified |= modifyDelta(subDeltaJson)
        })
    }
    return modified
}

readdirSync("./", { recursive: true })
    .filter((path) => path.endsWith(".delta.json"))
    .forEach((path) => {
        const deltaJson = JSON.parse(readFileSync(path, {encoding: "utf8"}))
        const modified = modifyDelta(deltaJson)
        writeFileSync(path, JSON.stringify(deltaJson, null, 2))
        console.log(`wrote back${modified ? ", with modifications" : ""}: ${path}`)
    })

