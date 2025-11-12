import type {
    ClocktowerServerDataMiniGuild
} from "@/api";

export type MiniGuild = {
    id: string;
    name: string;
};

export function mapToMiniGuild(apiMiniGuild:  ClocktowerServerDataMiniGuild | undefined): MiniGuild {
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