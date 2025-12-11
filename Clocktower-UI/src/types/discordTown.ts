import type {
    ClocktowerServerDataDiscordTownDto
} from "@/api";

import {
    mapToMiniCategory,
    type MiniCategory
} from "./miniCategory.ts";
import type {
    User
} from "@/types";

export type DiscordTown = {
    readonly gameId: string;
    readonly userCount: number;
    channelCategories: MiniCategory[];
}

export function mapToDiscordTown(apiDiscordTown: ClocktowerServerDataDiscordTownDto): DiscordTown {
    const channelCategories = (apiDiscordTown.channelCategories ?? [])
        .map(channelCategory => mapToMiniCategory(channelCategory));

    return {
        gameId: apiDiscordTown.gameId ?? '',
        userCount: apiDiscordTown.userCount ?? 0,
        channelCategories: channelCategories
    };
}

export function findGameUserById(discordTown: DiscordTown, userId: string): User | undefined {
    for (const category of discordTown.channelCategories) {
        for (const channelOccupant of category.channels) {
            const user = channelOccupant.occupants.find(occupant => occupant.id === userId);
            if (user) {
                return user;
            }
        }
    }
    return undefined;
}