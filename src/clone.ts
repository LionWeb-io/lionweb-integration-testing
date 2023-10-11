import {exec} from "./deps.ts"
import {config, fromRoot} from "./config.ts"


const token = Deno.env.get("LIONWEB_PAT")


const cloneRepo = (name: string, repoId: string) => {
    const localRepoDir = fromRoot(repoId)
    const url = `https://${token === undefined ? "" : `${token}@`}github.com/LionWeb-io/${repoId}`
    return exec(`rm -rf ${localRepoDir}`)
        .then(() => exec(`git clone --depth 1 ${url} ${localRepoDir}`))
            // Note: Git exits silently on failure!
        .then(() => {
            console.log(`cloned ${name}-repo (${repoId}) -> ${localRepoDir}`)
        })
}
// TODO  error handling


Promise.all(
        config.repositories
            .map(({ name, id }) => cloneRepo(name, id))
    )
    .then(() => {
        console.log(`\nDone cloning all LionWeb repositories.`)
    })

