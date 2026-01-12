import type {Role} from "@/types";

export const TokenRoleIcon = ({role}: { role: Role }) => {
    const iconUrl = new URL(`../../../public/tokenParts/roles/${role.id}.png`, import.meta.url).href;
    return (
        <span className="token-icon" style={{backgroundImage: `url(${iconUrl})`}}/>
    )
};