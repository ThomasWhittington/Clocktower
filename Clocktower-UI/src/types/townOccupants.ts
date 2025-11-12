import type {
    ClocktowerServerDataTownOccupants
} from "@/api";

import {
    mapToMiniCategory,
    type MiniCategory
} from "./miniCategory.ts";

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