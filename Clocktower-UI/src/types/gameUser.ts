import type {
    ClocktowerServerDataGameUser
} from "@/api";

export type GameUser = {
    id: string;
    name: string;
    avatarUrl: string | null;
};

export function mapToGameUser(apiGameUser: ClocktowerServerDataGameUser): GameUser {
    return {
        id: apiGameUser.id!,
        name: apiGameUser.name!,
        avatarUrl: apiGameUser.avatarUrl ?? null
    };
}