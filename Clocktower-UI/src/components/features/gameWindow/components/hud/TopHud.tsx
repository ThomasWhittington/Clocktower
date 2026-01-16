interface TopHudProps {
    scriptName: string | undefined
}

export const TopHud = ({scriptName}: TopHudProps) => {
    return (
        <div className="controls-top">
            {scriptName &&
                <div className="script-label">
                    {scriptName}
                </div>
            }
        </div>
    );
}