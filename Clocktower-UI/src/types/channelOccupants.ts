import type {
    ClocktowerServerDataChannelOccupantsDto,
} from "@/api";
import {
    mapToMiniChannel,
    type MiniChannel
} from "./miniChannel.ts";

import {
    mapToUser,
    type User
} from "@/types";

export type ChannelOccupants = {
    channel: MiniChannel;
    occupants: User[]
}

export function mapToChannelOccupants(channelOccupantsDto: ClocktowerServerDataChannelOccupantsDto): ChannelOccupants {
    const channel = mapToMiniChannel(channelOccupantsDto.channel);
    const occupants = (channelOccupantsDto.occupants ?? []).map(user => mapToUser(user));
    return {
        channel: channel,
        occupants: occupants
    };
}