import type {
    ClocktowerServerDataMiniChannel
} from "@/api";

export type MiniChannel = {
    id: string;
    name: string;
};

export function mapToMiniChannel(apiMiniUser: ClocktowerServerDataMiniChannel | undefined): MiniChannel {
    if (!apiMiniUser) {
        return {
            id: '',
            name: "Unknown channel"
        };
    }
    return {
        id: apiMiniUser.id ?? '',
        name: apiMiniUser.name ?? "Unknown channel"
    };
}