import type {
    ChannelOccupants,
    DiscordTown,
    User
} from '@/types';

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
    },
    getPlayerCountFromDistribution(discordTown: DiscordTown | undefined): number {
        if (!discordTown?.defaultRoleDistribution) return 0;

        return discordTown.defaultRoleDistribution.townsfolk +
            discordTown.defaultRoleDistribution.outsiders +
            discordTown.defaultRoleDistribution.minions +
            discordTown.defaultRoleDistribution.demons;
    },
    getPlayerBySeatPosition(discordTown: DiscordTown | undefined, seatPosition: number | undefined): User | undefined {
        if (!discordTown || seatPosition == null) return undefined;
        return discordTown.players.find(player => player.seatingPosition === seatPosition);
    },
    getTotalHandsUp(discordTown: DiscordTown | undefined): number {
        if (!discordTown) return 0;
        return discordTown.players.reduce((total, player) => player.handUp ? total + 1 : total, 0);
    }
};