import {
    healthApi
} from "@/api";
import {
    apiClient
} from "@/api/api-client.ts";

async function health() {
    const {
        data,
        error
    } = await healthApi({client: apiClient});

    if (error) {
        console.error('Failed to verify health of server:', error);
        throw new Error(error.toString());
    }

    return {
        status: data?.status!,
        timeStamp: data?.timeStamp!
    };
}

export const adminService = {
    health
}