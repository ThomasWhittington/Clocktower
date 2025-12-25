import {type ClocktowerServerDataTypesEnumGameTime, getAuthDataApi, getGuildsWithUserApi, getJoinDataApi, inviteUserApi, moveUserToChannelApi, pingUserApi, sendToCottagesApi, sendToTownSquareApi, setTimeApi} from '@/api';
import {GameTime, mapToMiniGuild, type MiniGuild} from "@/types";
import {apiClient} from "@/api/api-client.ts";

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

async function sendToCottages(gameId: string) {
    const {
        data,
        error
    } = await sendToCottagesApi({
        client: apiClient,
        path: {
            gameId: gameId
        }
    });

    if (error) {
        console.error('Failed to send users to cottages:', error);
        throw new Error(getMessage(error));
    }

    return data;
}

async function sendToTownSquare(gameId: string) {
    const {
        data,
        error
    } = await sendToTownSquareApi({
        client: apiClient,
        path: {
            gameId: gameId
        }
    });

    if (error) {
        console.error('Failed to send users to town square:', error);
        throw new Error(getMessage(error));
    }

    return data;
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

const getMessage = (err: unknown): string =>
    err instanceof Error ? err.message
        : typeof err === "object" && err && typeof (err as any).message === "string" ? (err as any).message
            : "Unknown error";

export const discordService = {
    getGuildsWithUser,
    moveUserToChannel,
    getAuthData,
    inviteUser,
    getJoinData,
    setTime,
    pingUser,
    sendToCottages,
    sendToTownSquare
}