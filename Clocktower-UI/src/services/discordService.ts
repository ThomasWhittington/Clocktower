import {
    createPathBasedClient
} from "openapi-fetch";
import type {
    paths
} from "../openApi/clocktowerServer";

const client = createPathBasedClient<paths>();

async function checkGuild(id: bigint): Promise<{
    valid?: boolean
    name?: string | null
    message?: string | null
}> {
    const {
        data,
        error
    } = await client["/api/discord/{guildId}"].GET({
        params: {
            path: {"guildId": id}
        }
    });
    if (error) {
        console.error('Failed to check guildId:', error);
        throw new Error(error.toString());
    }

    return data ?? {
        valid: false,
        name: '',
        message: ''
    };
}

export const discordService = {
    checkGuild
}