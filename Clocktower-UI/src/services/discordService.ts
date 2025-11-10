import {
    checkGuildApi,
    type CheckGuildApiResponse,
    getAuthDataApi,
    getGuildsWithUserApi,
    getTownOccupancyApi,
    getTownStatusApi,
    type GetTownStatusApiResponse,
    inviteUserApi,
    moveUserToChannelApi,
    rebuildTownApi,
    type RebuildTownApiResponse
} from '@/generated';
import {
    mapToMiniGuild,
    mapToTownOccupants,
    type MiniGuild,
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

async function getGuildsWithUser(userId: string): Promise<MiniGuild[]> {
    const {
        data,
        error
    } = await getGuildsWithUserApi({
        path: {
            userId: userId
        }
    });

    if (error) {
        console.error('Failed to get guilds:', error);
        throw new Error(error.toString());
    }

    if (data) {
        return data.miniGuilds?.map(mapToMiniGuild) ?? [];
    }
    return [];
}

async function inviteUser(guildId: string, userId: string): Promise<boolean> {
    const {
        data,
        error
    } = await inviteUserApi({
        path: {
            guildId: guildId,
            userId: userId
        }
    });

    if (error) {
        console.error('Failed to get guilds:', error);
        throw new Error(error.toString());
    }

    return data ?? false;
}

export const discordService = {
    checkGuild,
    getGuildsWithUser,
    getTownStatus,
    rebuildTown,
    getTownOccupancy,
    moveUserToChannel,
    getAuthData,
    inviteUser
}