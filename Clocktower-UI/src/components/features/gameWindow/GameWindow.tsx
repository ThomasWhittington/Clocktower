import {TownSquare} from "@/components/features";
import {useAppStore} from "@/store";
import {
    BottomHud,
    CenterHud,
    NightOrderPanel,
    RightHud,
    RoleListPanel,
    ScriptManagerPanel,
    StoryTellerHud,
    TokenPanel,
    TopHud,
    UserManagerPanel
} from "@/components/features/gameWindow/components";
import {UserUtils} from "@/utils";
import {
    useDiscordTown,
    useUser
} from "@/components/features/discordTownPanel/hooks";
import {
    useKeyboardShortcut,
    useServerHub
} from "@/hooks";
import {
    useActivePanel,
    useSetRoles
} from "@/components/features/gameWindow/hooks";
import {useState} from "react";
import {User} from "@/types";

export default function GameWindow() {
    const {gameId, currentUser} = useAppStore();
    const {thisUser} = useUser(currentUser?.id);
    const {discordTown} = useDiscordTown();
    const {script} = useServerHub();
    const [isDraftMode, setIsDraftMode] = useState(false);
    const {togglePanel, isPanelOpen, closePanel, openPanel, getPanelData} = useActivePanel();
    const {setRole, commitDraftRoles} = useSetRoles(currentUser?.id ?? "", UserUtils.isStoryTeller(thisUser), isDraftMode);

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
        onKeyPress: () => setIsDraftMode(prev => !prev),
        enabled: UserUtils.isStoryTeller(thisUser)
    });
    const tokenData = getPanelData('token');
    const handleCommitDraftRoles = async () => {
        await commitDraftRoles();
        setIsDraftMode(false);
    };

    return (
        <div className="game-window-controls">
            <TownSquare
                showDraftRoles={isDraftMode}
                onTokenClick={(player: User) => {
                    openPanel('token', {player});
                }}
                onCommitDraftRoles={isDraftMode ? handleCommitDraftRoles : undefined}
            />

            <UserManagerPanel isOpen={isPanelOpen('user')} onClose={closePanel}/>
            <ScriptManagerPanel isOpen={isPanelOpen('script')} onClose={closePanel}/>
            <RoleListPanel isOpen={isPanelOpen('role')} onClose={closePanel}/>
            <NightOrderPanel isOpen={isPanelOpen('night')} onClose={closePanel}/>
            {tokenData && (
                <TokenPanel
                    isOpen={isPanelOpen('token')}
                    onClose={closePanel}
                    player={tokenData.player}
                    isDraftMode={isDraftMode}
                    setRole={setRole}
                />
            )}
            {UserUtils.isStoryTeller(thisUser) &&
                <StoryTellerHud
                    usersIsOpen={isPanelOpen('user')}
                    onUsersClick={() => togglePanel('user')}
                    scriptIsOpen={isPanelOpen('script')}
                    onScriptClick={() => togglePanel('script')}
                    showDraftRoles={isDraftMode}
                    onDraftToggle={() => setIsDraftMode(prev => !prev)}
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
