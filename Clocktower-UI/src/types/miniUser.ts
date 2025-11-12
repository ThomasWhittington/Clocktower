import type {
    ClocktowerServerDataMiniUser
} from "@/api";

export type MiniUser = {
    id: string;
    name: string;
};

export function mapToMiniUser(apiMiniUser: ClocktowerServerDataMiniUser): MiniUser {
    return {
        id: apiMiniUser.id ?? '',
        name: apiMiniUser.name ?? "Unknown user"
    };
}