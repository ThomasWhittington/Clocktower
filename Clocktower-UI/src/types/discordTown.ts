import {type MiniCategory} from "./miniCategory.ts";
import {type User, UserType} from "@/types";

export class DiscordTown {
    readonly gameId: string;
    readonly townUsers: User[];
    readonly gameUsers: User[];
    channelCategories: MiniCategory[];

    constructor(data: Partial<DiscordTown>) {
        this.gameId = data.gameId ?? '';
        this.townUsers = data.townUsers ?? [];
        this.gameUsers = data.gameUsers ?? [];
        this.channelCategories = data.channelCategories ?? [];
    }

    get players(): User[] {
        return this.gameUsers.filter(u => u.userType === UserType.Player) ?? [];
    }

    get storyTellers(): User[] {
        return this.gameUsers.filter(u => u.userType === UserType.StoryTeller) ?? [];
    }

    get spectators(): User[] {
        return this.gameUsers.filter(u => u.userType === UserType.Spectator) ?? [];
    }
}

export function findGameUserById(discordTown?: DiscordTown, userId?: string): User | undefined {
    if (!discordTown || !userId) return undefined;
    for (const category of discordTown.channelCategories) {
        for (const channelOccupant of category.channels) {
            const user = channelOccupant.occupants.find(occupant => occupant.id === userId);
            if (user) {
                return user;
            }
        }
    }
    return undefined;
}