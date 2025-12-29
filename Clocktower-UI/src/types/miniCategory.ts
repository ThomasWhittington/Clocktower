import type {ClocktowerServerDataDtoMiniCategoryDto,} from "@/api";
import {type ChannelOccupants, mapToChannelOccupants} from "./channelOccupants.ts";

export type MiniCategory = {
    id: string;
    name: string;
    channels: ChannelOccupants[];
}

export function mapToMiniCategory(miniCategoryDto: ClocktowerServerDataDtoMiniCategoryDto): MiniCategory {
    const channels = (miniCategoryDto.channels ?? [])
        .map(channel => mapToChannelOccupants(channel));

    return {
        id: miniCategoryDto.id ?? '',
        name: miniCategoryDto.name ?? '',
        channels: channels
    };
}