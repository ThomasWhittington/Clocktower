import type {
    ClocktowerServerDiscordTownServicesMiniCategory,
} from "@/generated";
import {
    type ChannelOccupants,
    mapToChannelOccupants
} from "./channelOccupants.ts";

export type MiniCategory = {
    id: string;
    name: string;
    channels: ChannelOccupants[];
}

export function mapToMiniCategory(apiMiniCategory: ClocktowerServerDiscordTownServicesMiniCategory): MiniCategory {
    const channels = (apiMiniCategory.channels ?? [])
        .map(channel => mapToChannelOccupants(channel));

    return {
        id: apiMiniCategory.id ?? '',
        name: apiMiniCategory.name ?? '',
        channels: channels
    };
}