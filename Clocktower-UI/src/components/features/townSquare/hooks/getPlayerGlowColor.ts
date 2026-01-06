import type {ColorKey} from "@/theme/colors";
import {DiscordTown, type User} from "@/types";
import {DiscordTownUtils} from "@/utils";

interface GetPlayerGlowColorProps {
    player: User;
    currentUser: User | undefined;
    discordTown: DiscordTown | undefined;
}

export function getPlayerGlowColor({player, currentUser, discordTown}: GetPlayerGlowColorProps): ColorKey | undefined {
    if (player.id === currentUser?.id) {
        return "discordPrimary";
    }
    if (discordTown === undefined || currentUser === undefined) return undefined;

    const usersInSameChannel = DiscordTownUtils.getUsersInSameChannel(discordTown, currentUser.id);
    if (usersInSameChannel.some(user => user.id === player.id)) {
        return "forestgreen";
    }

    return undefined;
}