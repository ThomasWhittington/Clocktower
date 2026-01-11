export const RoleName = ({roleName}: { roleName: string }) => {
        const fontSize = (roleName && roleName.length > 10 ? "90%" : "110%");
        return (
            <svg viewBox="0 0 150 150" className="token-role-name">
                <path
                    d="M 13 75 C 13 160, 138 160, 138 75"
                    id="curve"
                    fill="transparent"
                />
                <text width="150" x="66.6%" text-anchor="middle" className="label" fontSize={fontSize}>
                    <textPath xlinkHref="#curve">
                        {roleName}
                    </textPath>
                </text>
            </svg>)
    }
;


