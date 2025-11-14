import type {
    ClocktowerServerDataMiniChannel
} from "@/api";

export type MiniChannel = {
    id: string;
    name: string;
};

export function mapToMiniChannel(apiMiniChannel: ClocktowerServerDataMiniChannel | undefined): MiniChannel {
    if (!apiMiniChannel) {
        return {
            id: '',
            name: "Unknown channel"
        };
    }
    return {
        id: apiMiniChannel.id ?? '',
        name: apiMiniChannel.name ?? "Unknown channel"
    };
}