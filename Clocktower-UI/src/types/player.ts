import type {
    ClocktowerServerDataTypesPlayer
} from "@/openApi";

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