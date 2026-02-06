import type {User} from "@/types";
import {Quill} from "@/components/ui/icons";
import {requestToTalk} from "@/hooks";
import {useAppStore} from "@/store";

interface BottomHudProps {
    storyTellers: User[],
    scriptName: string | undefined
}

export const BottomHud = ({scriptName, storyTellers}: BottomHudProps) => {
    const {gameId, currentUser} = useAppStore();
    if (!gameId || !currentUser) return null;
    const handleRequestToTalk = async (storyTellerId: string) => {
        await requestToTalk(gameId, currentUser.id, storyTellerId);
    };

    return (
        <div className="controls-bottom">
            {storyTellers.map((storyTeller) => (
                <div key={storyTeller.id} className="story-teller-label">
                    {storyTeller.id !== currentUser.id &&
                        <button
                            onClick={() => handleRequestToTalk(storyTeller.id)}
                            className="pointer-events-auto cursor-pointer"
                            title="Request to talk"
                        >
                            🗣
                        </button>
                    }
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