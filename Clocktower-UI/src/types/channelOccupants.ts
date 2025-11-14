import type {
    ClocktowerServerDataChannelOccupants,
} from "@/api";
import {
    mapToMiniChannel,
    type MiniChannel
} from "./miniChannel.ts";

import {
    type GameUser,
    mapToGameUser
} from "@/types/gameUser.ts";

export type ChannelOccupants = {
    channel: MiniChannel;
    occupants: GameUser[]
}

export function mapToChannelOccupants(apiChannelOccupants: ClocktowerServerDataChannelOccupants): ChannelOccupants {
    const channel = mapToMiniChannel(apiChannelOccupants.channel);
    const occupants = (apiChannelOccupants.occupants ?? []).map(user => mapToGameUser(user));
    return {
        channel: channel,
        occupants: occupants
    };
}