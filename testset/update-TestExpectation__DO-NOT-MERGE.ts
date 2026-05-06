const { readdirSync, readFileSync, writeFileSync } = require("fs")


const files = readdirSync(".", { recursive: true })
    .filter((path) => path.endsWith("/__TestExpectation.json"))

files.forEach((path) => {
    const json = JSON.parse(readFileSync(path, { encoding: "utf8" }))
    delete json.nodes
    writeFileSync(path, JSON.stringify(json, null, 2) + "\n")
})

