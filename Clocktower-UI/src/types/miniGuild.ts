import type {
    ClocktowerServerDiscordServicesDiscordServiceMiniGuild
} from "@/generated";

export type MiniGuild = {
    id: string;
    name: string;
};

export function mapToMiniGuild(apiMiniGuild:  ClocktowerServerDiscordServicesDiscordServiceMiniGuild | undefined): MiniGuild {
    if (!apiMiniGuild) {
        return {
            id: '',
            name: "Unknown guild"
        };
    }
    return {
        id: apiMiniGuild.id ?? '',
        name: apiMiniGuild.name ?? "Unknown guild"
    };
}