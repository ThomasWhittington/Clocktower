import type {
    ClocktowerServerDataDiscordTown
} from "@/api";

import {
    mapToMiniCategory,
    type MiniCategory
} from "./miniCategory.ts";
import type {
    TownUser
} from "@/types";

export type DiscordTown = {
    readonly userCount: number;
    channelCategories: MiniCategory[];
}

export function mapToDiscordTown(apiDiscordTown: ClocktowerServerDataDiscordTown): DiscordTown {
    const channelCategories = (apiDiscordTown.channelCategories ?? [])
        .map(channelCategory => mapToMiniCategory(channelCategory));

    return {
        userCount: apiDiscordTown.userCount ?? 0,
        channelCategories: channelCategories
    };
}

export function findGameUserById(discordTown: DiscordTown, userId: string): TownUser | undefined {
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