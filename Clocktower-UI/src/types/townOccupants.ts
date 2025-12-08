import type {
    ClocktowerServerDataTownOccupants
} from "@/api";

import {
    mapToMiniCategory,
    type MiniCategory
} from "./miniCategory.ts";
import type {
    GameUser
} from "@/types/gameUser.ts";

export type TownOccupants = {
    readonly userCount: number;
    channelCategories: MiniCategory[];
}

export function mapToTownOccupants(apiTownOccupants: ClocktowerServerDataTownOccupants): TownOccupants {
    const channelCategories = (apiTownOccupants.channelCategories ?? [])
        .map(channelCategory => mapToMiniCategory(channelCategory));

    return {
        userCount: apiTownOccupants.userCount ?? 0,
        channelCategories: channelCategories
    };
}

export function findGameUserById(townOccupants: TownOccupants, userId: string): GameUser | undefined {
    for (const category of townOccupants.channelCategories) {
        for (const channelOccupant of category.channels) {
            const user = channelOccupant.occupants.find(occupant => occupant.id === userId);
            if (user) {
                return user;
            }
        }
    }
    return undefined;
}