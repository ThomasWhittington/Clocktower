import type {User} from "@/types";
import {Quill} from "@/components/ui/icons";

interface BottomHudProps {
    gameId: string | null,
    storyTellers: User[]
}

export const BottomHud = ({gameId, storyTellers}: BottomHudProps) => {
    return (
        <div className="controls-bottom">
            {storyTellers.map((storyTeller) => (
                <div key={storyTeller.id} className="story-teller-label">
                    <Quill/>
                    {storyTeller.name}
                </div>
            ))}
            {gameId &&
                <div className="game-id-label">
                    {gameId}
                </div>
            }
        </div>
    );
}