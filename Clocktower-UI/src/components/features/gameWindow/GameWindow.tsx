import {TownSquare} from "@/components/features";
import {useAppStore} from "@/store";
import {useState} from "react";
import {BottomHud, CenterHud, StoryTellerHud, TopHud, UserManagerPanel} from "@/components/features/gameWindow/components";
import {UserUtils} from "@/utils";
import {useDiscordTown, useUser} from "@/components/features/discordTownPanel/hooks";
import {useKeyboardShortcut} from "@/hooks";

export default function GameWindow() {
    const {gameId, currentUser} = useAppStore();
    const [isInviteModalOpen, setIsInviteModalOpen] = useState(false);
    const {thisUser} = useUser(currentUser?.id);
    const {discordTown} = useDiscordTown();
    useKeyboardShortcut({
        key: 'u',
        onKeyPress: () => setIsInviteModalOpen(prev => !prev),
        enabled: UserUtils.isStoryTeller(thisUser)
    });

    return (
        <div className="game-window-controls">
            <TownSquare/>

            <UserManagerPanel
                isOpen={isInviteModalOpen}
                onClose={() => setIsInviteModalOpen(false)}
            />

            {UserUtils.isStoryTeller(thisUser) &&
                <StoryTellerHud
                    inviteIsOpen={isInviteModalOpen}
                    onInviteClick={() => setIsInviteModalOpen(prev => !prev)}
                />
            }
            <CenterHud/>
            <TopHud/>
            <BottomHud gameId={gameId} storyTellers={discordTown?.storyTellers ?? []}/>
        </div>
    );
};
