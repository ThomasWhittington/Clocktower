import {
    type ClientOptions,
} from "@/api";
import {
    createClient,
    createConfig
} from "@/api/generated/client";
import {
    useAppStore
} from "@/store";

export const apiClient =
    createClient(createConfig<ClientOptions>({
                baseUrl: import.meta.env.VITE_CLOCKTOWER_SERVER_URI,
                async fetch(input, init) {
                    const jwt = useAppStore.getState().jwt;
                    const headers = new Headers(init?.headers);

                    if (jwt) {
                        headers.set('Authorization', `Bearer ${jwt}`);
                    }

                    return fetch(input, {
                        ...init,
                        headers
                    });
                },
            }
        )
    );