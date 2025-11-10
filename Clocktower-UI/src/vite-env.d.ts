/// <reference types="vite/client" />

interface ImportMetaEnv {
    readonly CLOCKTOWER_SERVER_URI: string
    // add more env variables here as needed
}

interface ImportMeta {
    readonly env: ImportMetaEnv
}

declare namespace NodeJS {
    interface ProcessEnv {
        CLOCKTOWER_SERVER_URI?: string
    }
}