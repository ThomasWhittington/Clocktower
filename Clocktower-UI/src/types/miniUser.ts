import type {
    ClocktowerServerDiscordServicesMiniUser
} from "../openApi";

export type MiniUser = {
    id: string;
    name: string;
};

export function mapToMiniUser(apiMiniUser: ClocktowerServerDiscordServicesMiniUser): MiniUser {
    return {
        id: apiMiniUser.id ?? '',
        name: apiMiniUser.name ?? "Unknown user"
    };
}