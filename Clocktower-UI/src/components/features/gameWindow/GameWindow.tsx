import {TownSquare} from "@/components/features";
import {useAppStore} from "@/store";
import {
    GameHud,
    GamePanels
} from "@/components/features/gameWindow/components";
import {
    useCurrentUserIsStoryteller,
    useDiscordTown,
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
import {useTownSquareActions} from "@/components/features/townSquare/hooks";
import TalkRequestsBox from "@/components/features/gameWindow/components/hud/TalkRequestBox.tsx";

export default function GameWindow() {
    const {currentUser} = useAppStore();
    const {script} = useServerHub();
    const {discordTown} = useDiscordTown();
    const [isDraftMode, setIsDraftMode] = useState(false);
    const [selectedDraftRoles, setSelectedDraftRoles] = useState<Role[]>([]);
    const [circleDiameter, setCircleDiameter] = useState(0);
    const {togglePanel, isPanelOpen, closePanel, openPanel, getPanelData} = useActivePanel();
    const isStoryteller = useCurrentUserIsStoryteller();
    const {setRole, commitDraftRoles} = useSetRoles(currentUser?.id ?? "", isStoryteller, isDraftMode);

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
        script,
        togglePanel,
        setIsDraftMode,
        assignToDraft,
        selectedDraftRoles
    });

    const townSquareActions = useTownSquareActions();

    const handleTokenClick = (player: User) => {
        const canOpenToken = script && (isStoryteller || player.role?.type !== RoleType.Traveller);
        if (canOpenToken) {
            openPanel('token', {player});
        }
    };
    const handleManageRemindersClicked = (player: User) => {
        if (script) {
            console.log('Manage reminders clicked');
            openPanel('reminder', {player});
        }
    }
    const removeReminderClicked = (playerId: string, reminderId: string) => {
        console.log('Remove reminder clicked', playerId, reminderId);
    }

    const handleCommitDraftRoles = async () => {
        await commitDraftRoles();
        setIsDraftMode(false);
    };

    const tokenData = getPanelData('token');
    const reminderData = getPanelData('reminder');
    return (
        <div className="game-window-controls">
            <TownSquare
                showDraftRoles={isDraftMode}
                onTokenClick={handleTokenClick}
                onCommitDraftRoles={isDraftMode ? handleCommitDraftRoles : undefined}
                onCircleSizeChange={setCircleDiameter}
                townSquareActions={townSquareActions}
                onManageRemindersClicked={handleManageRemindersClicked}
                removeReminderClicked={removeReminderClicked}
            />

            <GamePanels
                isPanelOpen={isPanelOpen}
                closePanel={closePanel}
                tokenData={tokenData}
                reminderData={reminderData}
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
                circleDiameter={circleDiameter}
                onNominateClick={townSquareActions.initiateNomination}
                onVoteClick={townSquareActions.togglePlayerVote}
                onCancelNomination={townSquareActions.cancelNomination}
            />

            <TalkRequestsBox/>
        </div>
    );
};
