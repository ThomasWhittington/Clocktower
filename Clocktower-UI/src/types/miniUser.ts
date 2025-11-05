import type {
    ClocktowerServerDiscordTownServicesMiniUser
} from "@/generated";

export type MiniUser = {
    id: string;
    name: string;
};

export function mapToMiniUser(apiMiniUser: ClocktowerServerDiscordTownServicesMiniUser): MiniUser {
    return {
        id: apiMiniUser.id ?? '',
        name: apiMiniUser.name ?? "Unknown user"
    };
}