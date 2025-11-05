import {
    checkGuildApi,
    type CheckGuildApiResponse,
    getAuthDataApi,
    getTownOccupancyApi,
    getTownStatusApi,
    type GetTownStatusApiResponse,
    moveUserToChannelApi,
    rebuildTownApi,
    type RebuildTownApiResponse
} from '@/generated';
import {
    mapToTownOccupants,
    type TownOccupants
} from "@/types";


async function checkGuild(id: string): Promise<CheckGuildApiResponse> {
    const {
        data,
        error
    } = await checkGuildApi({
        path: {
            guildId: id.toString()
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

async function getTownStatus(id: string): Promise<GetTownStatusApiResponse> {
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

async function rebuildTown(id: string): Promise<RebuildTownApiResponse> {
    const {
        data,
        error
    } = await rebuildTownApi({
        path: {
            guildId: id
        }
    });

    if (error) {
        console.error('Failed to rebuild town:', error);
        throw new Error(error.toString());
    }

    return data ?? '';
}

async function getTownOccupancy(id: string): Promise<TownOccupants> {
    const {
        data,
        error
    } = await getTownOccupancyApi({
        path: {
            guildId: id
        }
    });

    if (error) {
        console.error('Failed to rebuild town:', error);
        throw new Error(error.toString());
    }


    if (data) {
        return mapToTownOccupants(data);
    }
    return {
        userCount: 0,
        channelCategories: []
    };
}

async function moveUserToChannel(guildId: string, userId: string, channelId: string): Promise<string> {
    const {
        data,
        error
    } = await moveUserToChannelApi({
        path: {
            guildId: guildId,
            userId: userId,
            channelId: channelId
        }
    });
    if (error) {
        console.error('Failed to move user to channel:', error);
        throw new Error(error.toString());
    }


    return data ?? 'Failed to move user to channel';
}

async function getAuthData(key: string) {
    return await getAuthDataApi({
        path: {
            key: key
        }
    });
}

export const discordService = {
    checkGuild,
    getTownStatus,
    rebuildTown,
    getTownOccupancy,
    moveUserToChannel,
    getAuthData
}