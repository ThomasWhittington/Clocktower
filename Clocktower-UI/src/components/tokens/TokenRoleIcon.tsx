export const TokenRoleIcon = ({roleId, className = "token-icon"}: { roleId: string, className?: string }) => {
    const iconUrl = `${import.meta.env.BASE_URL}tokenParts/roles/${roleId}.png`;

    return (
        <span className={className} style={{backgroundImage: `url(${iconUrl})`}}/>
    );
};