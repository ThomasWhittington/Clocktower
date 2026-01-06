import type {ChannelOccupants, DiscordTown, User} from '@/types';

export const DiscordTownUtils = {
    getUsersInSameChannel(discordTown: DiscordTown, userId: string): User[] {
        const channel = this.getUserChannel(discordTown, userId);
        return channel ? channel.occupants : [];
    },

    getUserChannel(discordTown: DiscordTown, userId: string): ChannelOccupants | undefined {
        const userIdStr = userId.toString();
        for (const category of discordTown.channelCategories) {
            const channel = category.channels.find(channel =>
                channel.occupants.some(occupant => occupant.id.toString() === userIdStr)
            );
            if (channel) return channel;
        }
        return undefined;
    }
};