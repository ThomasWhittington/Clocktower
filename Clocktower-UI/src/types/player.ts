import type {
    ClocktowerServerDataTypesPlayer
} from "@/generated";

export type Player = {
    id: number;
    name: string;
};

export function mapToPlayer(apiPlayer: ClocktowerServerDataTypesPlayer): Player {
    return {
        id: apiPlayer.id ?? 0,
        name: apiPlayer.name ?? "Unknown player"
    };
}