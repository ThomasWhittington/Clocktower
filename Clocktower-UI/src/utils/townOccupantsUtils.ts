import type {
    ChannelOccupants,
    TownOccupants
} from '@/types';

export const TownOccupantsUtils = {
    containsUser(townOccupancy: TownOccupants, userId: string | number): boolean {
        const userIdStr = userId.toString();
        return townOccupancy.channelCategories.some(category =>
            category.channels.some(channel =>
                channel.occupants.some(occupant =>
                    occupant.id.toString() === userIdStr
                )
            )
        );
    },

    getUserChannel(townOccupancy: TownOccupants, userId: string | number): ChannelOccupants | undefined {
        const userIdStr = userId.toString();
        for (const category of townOccupancy.channelCategories) {
            const channel = category.channels.find(channel =>
                channel.occupants.some(occupant => occupant.id.toString() === userIdStr)
            );
            if (channel) return channel;
        }
        return undefined;
    },

    getUsersInVoice(townOccupancy: TownOccupants): string[] {
        return townOccupancy.channelCategories.flatMap(category =>
            category.channels.flatMap(channel =>
                channel.occupants.map(occupant =>
                    occupant.id.toString()
                )
            )
        );
    }
};