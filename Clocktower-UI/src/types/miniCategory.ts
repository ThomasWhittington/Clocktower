import type {
    ClocktowerServerDiscordServicesMiniCategory,
} from "../openApi";
import {
    type ChannelOccupants,
    mapToChannelOccupants
} from "./channelOccupants.ts";

export type MiniCategory = {
    id: bigint;
    name: string;
    channels: ChannelOccupants[];
}

export function mapToMiniCategory(apiMiniCategory: ClocktowerServerDiscordServicesMiniCategory): MiniCategory {
    const channels = (apiMiniCategory.channels ?? [])
        .map(channel => mapToChannelOccupants(channel));

    return {
        id: apiMiniCategory.id ?? 0n,
        name: apiMiniCategory.name ?? '',
        channels: channels
    };
}