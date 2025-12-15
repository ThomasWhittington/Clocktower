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
            if (input instanceof Request) {
                const headers = new Headers(input.headers);
                
                if (jwt) {
                    headers.set('Authorization', `Bearer ${jwt}`);
                }
                
                const hasBody = input.body !== null;
                const hasContentType = headers.has('Content-Type');

                if (hasBody && !hasContentType) {
                    headers.set('Content-Type', 'application/json');
                }

                const patchedRequest = new Request(input, { headers });
                return fetch(patchedRequest);
            }

            const headers = new Headers(init?.headers);
            
            if (jwt) {
                headers.set('Authorization', `Bearer ${jwt}`);
            }

            const hasBody = init?.body !== undefined && init?.body !== null;
            const hasContentType = headers.has('Content-Type');

            const isFormData =
                typeof FormData !== 'undefined' && init?.body instanceof FormData;

            if (hasBody && !hasContentType && !isFormData) {
                headers.set('Content-Type', 'application/json');
            }

            return fetch(input, {
                ...init,
                headers
            });
        },
    }));