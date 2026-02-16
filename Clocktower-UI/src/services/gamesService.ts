import {
    type GamePerspective,
    GameTime,
    mapToGamePerspective,
    mapToUser,
    ScriptSelect,
    type User,
} from "@/types";
import {apiClient} from "@/api/api-client.ts";
import {
    addUserToGameApi,
    type ClocktowerServerDataTypesEnumGameTime,
    type ClocktowerServerDataTypesEnumScriptSelect,
    commitDraftRolesApi,
    getAvailableGameUsersApi,
    getGamesApi,
    randomiseSeatingPositionsApi,
    removeUserFromGameApi,
    setDraftRoleApi,
    setDraftRolesApi,
    setPerspectiveRoleApi,
    setPlayerHasVoteTokenApi,
    setPlayerIsDeadApi,
    setReminderApi,
    setRoleApi,
    setScriptApi,
    setTimeApi,
    startGameApi,
    swapSeatingPositionsApi
} from "@/api";

async function getGames(): Promise<GamePerspective[]> {

    const {
        data,
        error
    } = await getGamesApi({client: apiClient});

    if (error) {
        console.error('Failed to fetch games:', error);
        throw new Error('Failed to fetch games');
    }
    return data?.map(mapToGamePerspective) ?? [];
}

async function startGame(guildId: string, userId: string): Promise<GamePerspective | null> {
    const {
        data,
        error
    } = await startGameApi({
        client: apiClient,
        path: {
            guildId: guildId,
            userId: userId
        }
    });
    if (error) {
        console.error('Failed to start game:', error);
        throw new Error('Failed to start game');
    }

    if (!data) return null;
    return mapToGamePerspective(data);
}

async function getAvailableGameUsers(gameId: string): Promise<User[]> {
    const {
        data,
        error
    } = await getAvailableGameUsersApi({
        client: apiClient,
        path: {
            gameId: gameId,
        }
    });

    if (error) {
        console.error('Failed to get available users:', error);
        throw new Error(getMessage(error));
    }

    return data?.map(mapToUser) ?? [];
}

async function addUserToGame(gameId: string, userId: string): Promise<string> {
    const {
        data,
        error
    } = await addUserToGameApi({
        client: apiClient,
        path: {
            gameId: gameId,
            userId: userId
        }
    });

    if (error) {
        console.error('Failed to add user:', error);
        throw new Error(getMessage(error));
    }
    return data ?? '';
}

async function removeUserFromGame(gameId: string, userId: string): Promise<string> {
    const {
        data,
        error
    } = await removeUserFromGameApi({
        client: apiClient,
        path: {
            gameId: gameId,
            userId: userId
        }
    });

    if (error) {
        console.error('Failed to remove user:', error);
        throw new Error(getMessage(error));
    }

    return data ?? '';
}

async function randomiseSeatingPositions(gameId: string): Promise<string[]> {
    const {
        data,
        error
    } = await randomiseSeatingPositionsApi({
        client: apiClient,
        path: {
            gameId: gameId,
        }
    });

    if (error) {
        console.error('Failed to randomise seating positions:', error);
        throw new Error(getMessage(error));
    }

    return data ?? [];
}

async function swapSeatingPositions(gameId: string, userId1: string, userId2: string): Promise<string> {
    const {
        data,
        error
    } = await swapSeatingPositionsApi({
        client: apiClient,
        path: {
            gameId: gameId,
            userId1: userId1,
            userId2: userId2
        }
    });

    if (error) {
        console.error('Failed to swap player seats:', error);
        throw new Error(getMessage(error));
    }

    return data ?? '';
}

async function setPlayerIsDead(gameId: string, userId: string, isDead: boolean): Promise<string> {
    const {
        data,
        error
    } = await setPlayerIsDeadApi({
        client: apiClient,
        path: {
            gameId: gameId,
            userId: userId,
            isDead: isDead
        }
    });

    if (error) {
        console.error('Failed to set player dead state:', error);
        throw new Error(getMessage(error));
    }

    return data ?? '';
}

async function setPlayerHasVoteToken(gameId: string, userId: string, hasVoteToken: boolean): Promise<string> {
    const {
        data,
        error
    } = await setPlayerHasVoteTokenApi({
        client: apiClient,
        path: {
            gameId: gameId,
            userId: userId,
            hasVoteToken: hasVoteToken
        }
    });

    if (error) {
        console.error('Failed to set player has vote token state:', error);
        throw new Error(getMessage(error));
    }

    return data ?? '';
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
        throw new Error(getMessage(error));
    }
}

async function setScript(gameId: string, scriptSelect: ScriptSelect, json?: string) {
    const {
        error
    } = await setScriptApi({
        client: apiClient,
        path: {
            gameId: gameId,
        },
        query: {
            ScriptSelect: scriptSelectToString(scriptSelect),
            Json: json
        }
    });

    if (error) {
        console.error('Failed to set the game script:', error);
        throw new Error(getMessage(error));
    }
}

async function setPerspectiveRole(gameId: string, userId: string, targetUserId: string, roleId: string | undefined) {
    const {
        data,
        error
    } = await setPerspectiveRoleApi({
        client: apiClient,
        path: {
            gameId: gameId,
            userId: userId,
            targetUserId: targetUserId,
            roleId: roleId ?? ''
        }
    });

    if (error) {
        console.error('Failed to set perspective role for user:', error);
        throw new Error(getMessage(error));
    }

    return data;
}

async function setRole(gameId: string, targetUserId: string, roleId: string | undefined) {
    const {
        data,
        error
    } = await setRoleApi({
        client: apiClient,
        path: {
            gameId: gameId,
            targetUserId: targetUserId,
            roleId: roleId ?? ''
        }
    });

    if (error) {
        console.error('Failed to set role for user:', error);
        throw new Error(getMessage(error));
    }

    return data;
}

async function setDraftRole(gameId: string, targetUserId: string, roleId: string | undefined) {
    const {
        data,
        error
    } = await setDraftRoleApi({
        client: apiClient,
        path: {
            gameId: gameId,
            targetUserId: targetUserId,
            roleId: roleId ?? ''
        }
    });

    if (error) {
        console.error('Failed to set draft role for user:', error);
        throw new Error(getMessage(error));
    }

    return data;
}

async function commitDraftRoles(gameId: string) {
    const {
        data,
        error
    } = await commitDraftRolesApi({
        client: apiClient,
        path: {
            gameId: gameId
        }
    });

    if (error) {
        console.error('Failed to commit draft roles:', error);
        throw new Error(getMessage(error));
    }

    return data;
}

const gameTimeToString = (gameTime: GameTime): ClocktowerServerDataTypesEnumGameTime => {
    switch (gameTime) {
        case GameTime.Unknown:
            return 'Unknown';
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

async function setDraftRoles(gameId: string, playerRoles: Record<string, string>) {
    const {
        data,
        error
    } = await setDraftRolesApi({
        client: apiClient,
        path: {
            gameId: gameId
        },
        body: {
            playerRoles: playerRoles
        }
    });

    if (error) {
        console.error('Failed to set draft roles:', error);
        throw new Error(getMessage(error));
    }

    return data;
}

async function setReminder(gameId: string, userId: string, targetUserId: string, reminderId: string) {
    const {
        data,
        error
    } = await setReminderApi({
        client: apiClient,
        path: {
            gameId: gameId,
            userId: userId,
            targetUserId: targetUserId,
            reminderId: reminderId
        }
    });

    if (error) {
        console.error('Failed to set reminder for user:', error);
        throw new Error(getMessage(error));
    }

    return data;
}

const scriptSelectToString = (scriptSelect: ScriptSelect): ClocktowerServerDataTypesEnumScriptSelect => {
    switch (scriptSelect) {
        case ScriptSelect.Unknown:
            return "Unknown";
        case ScriptSelect.TroubleBrewing:
            return "TroubleBrewing";
        case ScriptSelect.SectsAndViolets:
            return "SectsAndViolets";
        case ScriptSelect.BadMoonRising:
            return "BadMoonRising";
        case ScriptSelect.Custom:
            return "Custom";
        default:
            throw new Error(`Unknown ScriptSelect value: ${scriptSelect}`);
    }
}
const getMessage = (err: unknown): string => {
    if (typeof err === "string") return err;
    if (err instanceof Error) return err.message;
    if (typeof err === "object" && err && typeof (err as any).message === "string") {
        return (err as any).message;
    }
    return "Unknown error";
};

export const gamesService = {
    getGames,
    startGame,
    getAvailableGameUsers,
    addUserToGame,
    removeUserFromGame,
    randomiseSeatingPositions,
    swapSeatingPositions,
    setPlayerIsDead,
    setPlayerHasVoteToken,
    setTime,
    setScript,
    setRole,
    setDraftRole,
    setDraftRoles,
    setPerspectiveRole,
    commitDraftRoles,
    setReminder
}