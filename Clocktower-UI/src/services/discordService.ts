import {
    checkGuildApi,
    type ClocktowerServerDiscordEndpointsGetTownStatusResponse,
    getTownStatusApi
} from '../openApi';

async function checkGuild(id: bigint) {
    const {
        data,
        error
    } = await checkGuildApi({
        path: {
            guildId: id,
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

async function getTownStatus(id: bigint): Promise<ClocktowerServerDiscordEndpointsGetTownStatusResponse> {
    const {
        data,
        error
    } = await getTownStatusApi({
        path: {
            guildId: id,
        }
    });

    if (error) {
        console.error('Failed to get town status:', error);
        throw new Error(error.toString());
    }
    return data ?? {
        exists: false,
        message: "Failed to get town status"
    };
}


export const discordService = {
    checkGuild,
    getTownStatus
}