import type {
    ClocktowerServerDiscordServicesMiniUser
} from "../openApi";

export type MiniUser = {
    id: bigint;
    name: string;
};

export function mapToMiniUser(apiMiniUser: ClocktowerServerDiscordServicesMiniUser): MiniUser {
    return {
        id: apiMiniUser.id ?? 0n,
        name: apiMiniUser.name ?? "Unknown user"
    };
}