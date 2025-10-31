import {
    checkGuildApi,
    type CheckGuildApiResponse,
    getTownStatusApi,
    type GetTownStatusApiResponse,
    rebuildTownApi,
    type RebuildTownApiResponse
} from '../openApi';

async function checkGuild(id: bigint): Promise<CheckGuildApiResponse> {
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

async function getTownStatus(id: bigint): Promise<GetTownStatusApiResponse> {
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

async function rebuildTown(id: bigint): Promise<RebuildTownApiResponse> {
    const {
        data,
        error
    } = await rebuildTownApi({path: {guildId: id}});
    if (error) {
        console.error('Failed to rebuild town:', error);
        throw new Error(error.toString());
    }

    return data ?? '';
}

export const discordService = {
    checkGuild,
    getTownStatus,
    rebuildTown
}