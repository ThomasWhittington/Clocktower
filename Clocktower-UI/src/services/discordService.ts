import {
    checkGuildApi,
    type CheckGuildApiResponse,
    type ClocktowerServerDataTypesEnumGameTime,
    getAuthDataApi,
    getGuildsWithUserApi,
    getJoinDataApi,
    getTownOccupancyApi,
    getTownStatusApi,
    type GetTownStatusApiResponse,
    inviteUserApi,
    moveUserToChannelApi,
    rebuildTownApi,
    type RebuildTownApiResponse,
    setTimeApi
} from '@/api';
import {
    mapToMiniGuild,
    mapToTownOccupants,
    type MiniGuild,
    type TownOccupants
} from "@/types";
import {
    apiClient
} from "@/api/api-client.ts";
import {
    GameTime
} from "@/components/features/discordTownPanel/hooks";

async function checkGuild(id: string): Promise<CheckGuildApiResponse> {
    const {
        data,
        error
    } = await checkGuildApi({
        client: apiClient,
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
        client: apiClient,
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
        client: apiClient,
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
        client: apiClient,
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
        client: apiClient,
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
        client: apiClient,
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
        client: apiClient,
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

async function inviteUser(gameId: string, userId: string): Promise<boolean> {
    const {
        error
    } = await inviteUserApi({
        client: apiClient,
        path: {
            gameId: gameId,
            userId: userId
        }
    });

    if (error) {
        console.error('Failed to invite user:', error);
        throw new Error(error.toString());
    }

    return true;
}

async function getJoinData(key: string) {
    return await getJoinDataApi({
        client: apiClient,
        path: {
            key: key
        }
    });
}

async function setTime(gameId: string, gameTime: GameTime) {
    const {
        error
    } = await setTimeApi({
        client: apiClient,
        path: {
            gameId: gameId,
        },
        query: {
            GameTime: gameTimeToString(gameTime)
        }
    });

    if (error) {
        console.error('Failed to set the game time:', error);
        throw new Error(error.toString());
    }
}

const gameTimeToString = (gameTime: GameTime): ClocktowerServerDataTypesEnumGameTime => {
    switch (gameTime) {
        case GameTime.Day:
            return 'Day';
        case GameTime.Evening:
            return 'Evening';
        case GameTime.Night:
            return 'Night';
        default:
            throw new Error(`Unknown GameTime value: ${gameTime}`);
    }
};
export const discordService = {
    checkGuild,
    getGuildsWithUser,
    getTownStatus,
    rebuildTown,
    getTownOccupancy,
    moveUserToChannel,
    getAuthData,
    inviteUser,
    getJoinData,
    setTime
}