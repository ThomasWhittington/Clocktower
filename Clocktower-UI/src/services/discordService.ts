import {type ClocktowerServerDataTypesEnumUserType, getAuthDataApi, getGuildsWithUserApi, getJoinDataApi, inviteAllApi, inviteUserApi, moveUserToChannelApi, sendToCottagesApi, sendToTownSquareApi, setUserTypeApi} from '@/api';
import {mapToMiniGuild, type MiniGuild, UserType} from "@/types";
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
        throw new Error(getMessage(error));
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
        throw new Error(getMessage(error));
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
        throw new Error(getMessage(error));
    }

    return true;
}

async function inviteAll(gameId: string): Promise<boolean> {
    const {
        error
    } = await inviteAllApi({
        client: apiClient,
        path: {
            gameId: gameId,
        }
    });

    if (error) {
        console.error('Failed to invite all:', error);
        throw new Error(getMessage(error));
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

async function setUserType(gameId: string, userId: string, userType: UserType) {
    const {
        data,
        error
    } = await setUserTypeApi({
        client: apiClient,
        path: {
            gameId: gameId,
            userId: userId,
            userType: mapUserType(userType)
        }
    });

    if (error) {
        console.error('Failed to set userType for user:', error);
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

const getMessage = (err: unknown): string => {
    const error = typeof err === "object" && err && typeof (err as any).message === "string" ? (err as any).message : "Unknown error";
    return err instanceof Error ? err.message : error;
};

function mapUserType(type: UserType): ClocktowerServerDataTypesEnumUserType {
    return type as unknown as ClocktowerServerDataTypesEnumUserType;
}

export const discordService = {
    getGuildsWithUser,
    moveUserToChannel,
    getAuthData,
    inviteUser,
    inviteAll,
    getJoinData,
    sendToCottages,
    sendToTownSquare,
    setUserType
}