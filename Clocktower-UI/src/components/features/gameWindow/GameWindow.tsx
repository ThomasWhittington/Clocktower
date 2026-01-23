import {TownSquare} from "@/components/features";
import {useAppStore} from "@/store";
import {
    GameHud,
    GamePanels
} from "@/components/features/gameWindow/components";
import {UserUtils} from "@/utils";
import {
    useDiscordTown,
    useUser
} from "@/components/features/discordTownPanel/hooks";
import {useServerHub} from "@/hooks";
import {
    useActivePanel,
    useAssignToDraft,
    useGameWindowShortcuts,
    useSetRoles
} from "@/components/features/gameWindow/hooks";
import {
    useEffect,
    useState
} from "react";
import {
    Role,
    RoleType,
    User
} from "@/types";

export default function GameWindow() {
    const {currentUser} = useAppStore();
    const {thisUser} = useUser(currentUser?.id);
    const {script} = useServerHub();
    const {discordTown} = useDiscordTown();
    const [isDraftMode, setIsDraftMode] = useState(false);
    const [selectedDraftRoles, setSelectedDraftRoles] = useState<Role[]>([]);
    const {togglePanel, isPanelOpen, closePanel, openPanel, getPanelData} = useActivePanel();
    const {setRole, commitDraftRoles} = useSetRoles(currentUser?.id ?? "", UserUtils.isStoryTeller(thisUser), isDraftMode);
    const isStoryteller = UserUtils.isStoryTeller(thisUser);

    useEffect(() => {
        if (script && isPanelOpen('script')) {
            closePanel();
        }
    }, [script]);

    const {assignToDraft} = useAssignToDraft({
        selectedRoles: selectedDraftRoles,
        discordTown,
        setIsDraftMode,
        closePanel
    });
    useGameWindowShortcuts({
        thisUser,
        script,
        togglePanel,
        setIsDraftMode,
        assignToDraft,
        selectedDraftRoles
    });
    const handleTokenClick = (player: User) => {
        const canOpenToken = script && (isStoryteller || player.role?.type !== RoleType.Traveller);
        if (canOpenToken) {
            openPanel('token', {player});
        }
    };

    const handleCommitDraftRoles = async () => {
        await commitDraftRoles();
        setIsDraftMode(false);
    };

    const tokenData = getPanelData('token');
    return (
        <div className="game-window-controls">
            <TownSquare
                showDraftRoles={isDraftMode}
                onTokenClick={handleTokenClick}
                onCommitDraftRoles={isDraftMode ? handleCommitDraftRoles : undefined}
            />

            <GamePanels
                isPanelOpen={isPanelOpen}
                closePanel={closePanel}
                tokenData={tokenData}
                isDraftMode={isDraftMode}
                setIsDraftMode={setIsDraftMode}
                setRole={setRole}
                selectedRoles={selectedDraftRoles}
                setSelectedRoles={setSelectedDraftRoles}
            />

            <GameHud
                script={script}
                isStoryteller={isStoryteller}
                isPanelOpen={isPanelOpen}
                togglePanel={togglePanel}
                setIsDraftMode={setIsDraftMode}
                isDraftMode={isDraftMode}
            />
        </div>
    );
};
