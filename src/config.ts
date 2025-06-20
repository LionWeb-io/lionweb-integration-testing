import {parse, readFileSync} from "./deps.ts"


type Config = {
    repositories: Repository[]
    serializations: Serialization[]
    schemas: Schema[]
}

type Repository = {
    name: string
    id: string
    // TODO  add Git ref
}

type RepositoryKind = | "Java" | "TypeScript"
type ModelKind = "m3"
type Serialization = {
    path: string
    kind: ModelKind
    repo: RepositoryKind
}

type Schema = {
    path: string
    kind: ModelKind
}

export type { Config, ModelKind, RepositoryKind, Schema, Serialization }


export const config = parse(readFileSync("config.yaml", {encoding: "utf-8"})) as Config

export const pathOfSerialization = (kind: ModelKind, repo: RepositoryKind): string =>
    config.serializations
        .find((serialization) => serialization.kind === kind && serialization.repo === repo)!
        .path

export const pathOfSchema = (kind: ModelKind): string =>
    config.schemas
        .find((schema) => schema.kind === kind)!
        .path


export const reposDir = "repos"

export const fromRoot = (path: string): string =>
    `../${reposDir}/${path}`

