import type {
    ClocktowerServerDiscordTownServicesTownOccupants
} from "@/generated";
import {
    mapToMiniCategory,
    type MiniCategory
} from "./miniCategory.ts";

export type TownOccupants = {
    readonly userCount: number;
    channelCategories: MiniCategory[];
}

export function mapToTownOccupants(apiTownOccupants: ClocktowerServerDiscordTownServicesTownOccupants): TownOccupants {
    const channelCategories = (apiTownOccupants.channelCategories ?? [])
        .map(channelCategory => mapToMiniCategory(channelCategory));

    return {
        userCount: apiTownOccupants.userCount ?? 0,
        channelCategories: channelCategories
    };
}