import {TownSquare} from "@/components/features";
import {useAppStore} from "@/store";
import {BottomHud, CenterHud, ScriptManagerPanel, StoryTellerHud, TopHud, UserManagerPanel} from "@/components/features/gameWindow/components";
import {UserUtils} from "@/utils";
import {useDiscordTown, useUser} from "@/components/features/discordTownPanel/hooks";
import {useKeyboardShortcut} from "@/hooks";
import {useActivePanel} from "@/components/features/gameWindow/hooks";

export default function GameWindow() {
    const {gameId, currentUser} = useAppStore();
    const {thisUser} = useUser(currentUser?.id);
    const {discordTown} = useDiscordTown();
    const {togglePanel, isPanelOpen, closePanel} = useActivePanel();

    useKeyboardShortcut({
        key: 'u',
        onKeyPress: () => togglePanel('user'),
        enabled: UserUtils.isStoryTeller(thisUser)
    });
    useKeyboardShortcut({
        key: 's',
        onKeyPress: () => togglePanel('script'),
        enabled: UserUtils.isStoryTeller(thisUser)
    });
    return (
        <div className="game-window-controls">
            <TownSquare/>

            <UserManagerPanel
                isOpen={isPanelOpen('user')}
                onClose={closePanel}
            />
            <ScriptManagerPanel
                isOpen={isPanelOpen('script')}
                onClose={closePanel}
            />

            {UserUtils.isStoryTeller(thisUser) &&
                <StoryTellerHud
                    inviteIsOpen={isPanelOpen('user')}
                    onInviteClick={() => togglePanel('user')}
                    scriptIsOpen={isPanelOpen('script')}
                    onScriptClick={() => togglePanel('script')}
                />
            }
            <CenterHud/>
            <TopHud/>
            <BottomHud gameId={gameId} storyTellers={discordTown?.storyTellers ?? []}/>
        </div>
    );
};
