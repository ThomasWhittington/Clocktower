import type {
    ClocktowerServerDataChannelOccupants,
} from "@/api";
import {
    mapToMiniChannel,
    type MiniChannel
} from "./miniChannel.ts";

import {
    mapToTownUser,
    type TownUser
} from "@/types";

export type ChannelOccupants = {
    channel: MiniChannel;
    occupants: TownUser[]
}

export function mapToChannelOccupants(apiChannelOccupants: ClocktowerServerDataChannelOccupants): ChannelOccupants {
    const channel = mapToMiniChannel(apiChannelOccupants.channel);
    const occupants = (apiChannelOccupants.occupants ?? []).map(user => mapToTownUser(user));
    return {
        channel: channel,
        occupants: occupants
    };
}