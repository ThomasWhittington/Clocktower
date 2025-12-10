import {
    type ClocktowerServerDataTypesEnumGameTime,
    getAuthDataApi,
    getDiscordTownApi,
    getGuildsWithUserApi,
    getJoinDataApi,
    getTownStatusApi,
    type GetTownStatusApiResponse,
    inviteUserApi,
    moveUserToChannelApi,
    pingUserApi,
    setTimeApi
} from '@/api';
import {
    type DiscordTown,
    GameTime,
    mapToDiscordTown,
    mapToMiniGuild,
    type MiniGuild
} from "@/types";
import {
    apiClient
} from "@/api/api-client.ts";

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

async function getDiscordTown(id: string): Promise<DiscordTown> {
    const {
        data,
        error
    } = await getDiscordTownApi({
        client: apiClient,
        path: {
            guildId: id
        }
    });

    if (error) {
        console.error('Failed to get Discord Town', error);
        throw new Error(error.toString());
    }


    if (data) {
        return mapToDiscordTown(data);
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

async function pingUser(userId: string): Promise<boolean> {
    const {
        error
    } = await pingUserApi({
        client: apiClient,
        path: {
            userId: userId
        }
    });

    if (error) {
        console.error('Failed to ping user:', error);
        throw new Error(error.toString());
    }

    return true;
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
    getGuildsWithUser,
    getTownStatus,
    getDiscordTown,
    moveUserToChannel,
    getAuthData,
    inviteUser,
    getJoinData,
    setTime,
    pingUser
}