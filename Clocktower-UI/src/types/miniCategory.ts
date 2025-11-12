import type {
    ClocktowerServerDataMiniCategory,
} from "@/api";
import {
    type ChannelOccupants,
    mapToChannelOccupants
} from "./channelOccupants.ts";

export type MiniCategory = {
    id: string;
    name: string;
    channels: ChannelOccupants[];
}

export function mapToMiniCategory(apiMiniCategory: ClocktowerServerDataMiniCategory): MiniCategory {
    const channels = (apiMiniCategory.channels ?? [])
        .map(channel => mapToChannelOccupants(channel));

    return {
        id: apiMiniCategory.id ?? '',
        name: apiMiniCategory.name ?? '',
        channels: channels
    };
}