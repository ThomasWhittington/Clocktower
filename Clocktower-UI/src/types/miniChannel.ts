import type {
    ClocktowerServerDiscordServicesMiniChannel
} from "../openApi";

export type MiniChannel = {
    id: bigint;
    name: string;
};

export function mapToMiniChannel(apiMiniUser: ClocktowerServerDiscordServicesMiniChannel | undefined): MiniChannel {
    if (!apiMiniUser) {
        return {
            id: 0n,
            name: "Unknown channel"
        };
    }
    return {
        id: apiMiniUser.id ?? 0n,
        name: apiMiniUser.name ?? "Unknown channel"
    };
}