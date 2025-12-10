import type {
    ChannelOccupants,
    DiscordTown
} from '@/types';

export const DiscordTownUtils = {
    containsUser(discordTown: DiscordTown, userId: string | number): boolean {
        const userIdStr = userId.toString();
        return discordTown.channelCategories.some(category =>
            category.channels.some(channel =>
                channel.occupants.some(occupant =>
                    occupant.id.toString() === userIdStr
                )
            )
        );
    },

    getUserChannel(discordTown: DiscordTown, userId: string | number): ChannelOccupants | undefined {
        const userIdStr = userId.toString();
        for (const category of discordTown.channelCategories) {
            const channel = category.channels.find(channel =>
                channel.occupants.some(occupant => occupant.id.toString() === userIdStr)
            );
            if (channel) return channel;
        }
        return undefined;
    },

    getUsersInVoice(discordTown: DiscordTown): string[] {
        return discordTown.channelCategories.flatMap(category =>
            category.channels.flatMap(channel =>
                channel.occupants.map(occupant =>
                    occupant.id.toString()
                )
            )
        );
    }
};