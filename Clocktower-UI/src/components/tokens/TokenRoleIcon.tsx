import type {Role} from "@/types";

export const TokenRoleIcon = ({role, className}: { role: Role, className?: string }) => {
    const iconUrl = new URL(`../../../public/tokenParts/roles/${role.id}.png`, import.meta.url).href;
    return (
        <span className={className} style={{backgroundImage: `url(${iconUrl})`}}/>
    )
};