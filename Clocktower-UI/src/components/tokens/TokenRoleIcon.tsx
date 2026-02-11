export const TokenRoleIcon = ({roleId, className = "token-icon"}: { roleId: string, className?: string }) => {
    const iconUrl = new URL(`../../../public/tokenParts/roles/${roleId}.png`, import.meta.url).href;
    return (
        <span className={className} style={{backgroundImage: `url(${iconUrl})`}}/>
    )
};