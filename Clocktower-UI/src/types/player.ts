import type {
    components
} from "../openApi/clocktowerServer";

export type Player = {
    id: number;
    name: string;
};
type OpenApiPlayer = components["schemas"]["Clocktower.Server.Data.Types.Player"];


export function mapToPlayer(apiPlayer: OpenApiPlayer): Player {
    return {
        id: apiPlayer.id ?? 0,
        name: apiPlayer.name ?? "Unknown player"
    };
}