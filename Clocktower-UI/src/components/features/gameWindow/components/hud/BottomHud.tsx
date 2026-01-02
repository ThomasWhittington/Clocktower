interface BottomHudProps {
    gameId: string | null
}

export const BottomHud = ({gameId}: BottomHudProps) => {
    return (
        <div className="controls-bottom">
            {gameId &&
                <div className="game-id-label">
                    <p className="text-sm">{gameId}</p>
                </div>
            }
        </div>
    );
}