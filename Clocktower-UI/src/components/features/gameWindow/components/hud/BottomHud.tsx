import type {User} from "@/types";
import {Quill} from "@/components/ui/icons";

interface BottomHudProps {
    storyTellers: User[]
}

export const BottomHud = ({storyTellers}: BottomHudProps) => {
    return (
        <div className="controls-bottom">
            {storyTellers.map((storyTeller) => (
                <div key={storyTeller.id} className="story-teller-label">
                    <Quill/>
                    {storyTeller.name}
                </div>
            ))}
        </div>
    );
}