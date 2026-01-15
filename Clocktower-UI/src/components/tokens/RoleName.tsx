import {useId} from 'react';

export const RoleName = ({roleName}: { roleName: string }) => {
    const curveId = useId();
    const fontSize = (roleName && roleName.length > 10 ? "80%" : "100%");
    return (
        <svg viewBox="0 0 150 150" className="token-role-name">
            <path
                d="M 13 75 C 13 160, 138 160, 138 75"
                id={curveId}
                fill="transparent"
            />
            <text width="150" x="66.6%" text-anchor="middle" className="label" fontSize={fontSize}>
                <textPath href={`#${curveId}`}>
                    {roleName}
                </textPath>
            </text>
        </svg>
    )
};


