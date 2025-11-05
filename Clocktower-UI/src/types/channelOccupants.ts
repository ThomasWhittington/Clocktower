import type {
    ClocktowerServerDiscordTownServicesChannelOccupants
} from "@/generated";
import {
    mapToMiniChannel,
    type MiniChannel
} from "./miniChannel.ts";

import {
    mapToMiniUser,
    type MiniUser
} from "./miniUser.ts";

export type ChannelOccupants = {
    channel: MiniChannel;
    occupants: MiniUser[]
}

export function mapToChannelOccupants(apiChannelOccupants: ClocktowerServerDiscordTownServicesChannelOccupants): ChannelOccupants {
    const channel = mapToMiniChannel(apiChannelOccupants.channel);
    const occupants = (apiChannelOccupants.occupants ?? []).map(user => mapToMiniUser(user));
    return {
        channel: channel,
        occupants: occupants
    };
}