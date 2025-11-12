import {
    type ClientOptions,
} from "@/api";
import {
    createClient,
    createConfig
} from "@/api/generated/client";

export const apiClient =
    createClient(createConfig<ClientOptions>({
        baseUrl: import.meta.env.VITE_CLOCKTOWER_SERVER_URI
    }));