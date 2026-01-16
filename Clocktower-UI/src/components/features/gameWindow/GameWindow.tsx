import {TownSquare} from "@/components/features";
import {useAppStore} from "@/store";
import {BottomHud, CenterHud, NightOrderPanel, RightHud, RoleListPanel, ScriptManagerPanel, StoryTellerHud, TopHud, UserManagerPanel} from "@/components/features/gameWindow/components";
import {UserUtils} from "@/utils";
import {useDiscordTown, useUser} from "@/components/features/discordTownPanel/hooks";
import {useKeyboardShortcut, useServerHub} from "@/hooks";
import {useActivePanel} from "@/components/features/gameWindow/hooks";
import {useState} from "react";

export default function GameWindow() {
    const {gameId, currentUser} = useAppStore();
    const {thisUser} = useUser(currentUser?.id);
    const {discordTown} = useDiscordTown();
    const {script} = useServerHub();
    const [showDraftRoles, setShowDraftRoles] = useState(false);
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
    useKeyboardShortcut({
        key: 'r',
        onKeyPress: () => togglePanel('role')
    });
    useKeyboardShortcut({
        key: 'n',
        onKeyPress: () => togglePanel('night')
    });
    useKeyboardShortcut({
        key: 'd',
        onKeyPress: () => setShowDraftRoles(prev => !prev),
        enabled: UserUtils.isStoryTeller(thisUser)
    });

    return (
        <div className="game-window-controls">
            <TownSquare showDraftRoles={showDraftRoles}/>

            <UserManagerPanel isOpen={isPanelOpen('user')} onClose={closePanel}/>
            <ScriptManagerPanel isOpen={isPanelOpen('script')} onClose={closePanel}/>
            <RoleListPanel isOpen={isPanelOpen('role')} onClose={closePanel}/>
            <NightOrderPanel isOpen={isPanelOpen('night')} onClose={closePanel}/>

            {UserUtils.isStoryTeller(thisUser) &&
                <StoryTellerHud
                    inviteIsOpen={isPanelOpen('user')}
                    onInviteClick={() => togglePanel('user')}
                    scriptIsOpen={isPanelOpen('script')}
                    onScriptClick={() => togglePanel('script')}
                    showDraftRoles={showDraftRoles}
                    onDraftToggle={() => setShowDraftRoles(prev => !prev)}
                />
            }
            <CenterHud/>
            <TopHud scriptName={script?.name}/>
            <RightHud
                onRoleListClick={() => togglePanel('role')}
                onNightOrderClick={() => togglePanel('night')}
            />
            <BottomHud gameId={gameId} storyTellers={discordTown?.storyTellers ?? []}/>
        </div>
    );
};
