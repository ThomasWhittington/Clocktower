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

declare module "*.svg?react" {
    import React
        from "react";
    const ReactComponent: React.FunctionComponent<React.SVGProps<SVGSVGElement>>;
    export default ReactComponent;
}

declare module "*.svg" {
    const src: string;
    export default src;
}