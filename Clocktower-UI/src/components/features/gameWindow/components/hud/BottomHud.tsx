import type {User} from "@/types";
import {Quill} from "@/components/ui/icons";

interface BottomHudProps {
    storyTellers: User[],
    scriptName: string | undefined
}

export const BottomHud = ({scriptName, storyTellers}: BottomHudProps) => {
    return (
        <div className="controls-bottom">
            {storyTellers.map((storyTeller) => (
                <div key={storyTeller.id} className="story-teller-label">
                    <Quill/>
                    {storyTeller.name}
                </div>
            ))}
            {scriptName &&
                <div className="script-label">
                    {scriptName}
                </div>
            }
        </div>
    );
}