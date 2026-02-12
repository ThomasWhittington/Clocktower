import {
    NightOrderPanel,
    PaperPanel,
    ReminderPanel,
    RoleListPanel,
    RolePlannerPanel,
    ScriptManagerPanel,
    TokenPanel,
    UserManagerPanel,
    VoteHistoryPanel
} from "./index";
import type {PanelType} from "@/components/features/gameWindow/hooks";
import {
    Role,
    User
} from "@/types";
import type {
    Dispatch,
    SetStateAction
} from "react";

interface TokenData {
    player: User;
}

interface GamePanelsProps {
    isPanelOpen: (panel: PanelType) => boolean,
    closePanel: () => void,
    tokenData: TokenData | null,
    isDraftMode: boolean,
    setRole: (role: Role | undefined, playerId: string) => Promise<void>,
    setIsDraftMode: (callback: (prev: boolean) => boolean) => void,
    selectedRoles: Role[],
    setSelectedRoles: Dispatch<SetStateAction<Role[]>>,
    addReminderClicked?: (playerId: string, reminderId: string) => void,
    reminderData?: { player: User } | null
}

export function GamePanels({isPanelOpen, closePanel, tokenData, isDraftMode, setRole, setIsDraftMode, setSelectedRoles, selectedRoles, addReminderClicked, reminderData}: Readonly<GamePanelsProps>) {
    return (
        <>
            <UserManagerPanel isOpen={isPanelOpen('user')} onClose={closePanel}/>
            <ScriptManagerPanel isOpen={isPanelOpen('script')} onClose={closePanel}/>
            <RoleListPanel isOpen={isPanelOpen('role')} onClose={closePanel}/>
            <NightOrderPanel isOpen={isPanelOpen('night')} onClose={closePanel}/>
            <VoteHistoryPanel isOpen={isPanelOpen('voteHistory')} onClose={closePanel}/>
            <PaperPanel isOpen={isPanelOpen('paper')} onClose={closePanel}/>
            <RolePlannerPanel isOpen={isPanelOpen('rolePlanner')} onClose={closePanel} setIsDraftMode={setIsDraftMode} setSelectedRoles={setSelectedRoles} selectedRoles={selectedRoles}/>

            {reminderData?.player &&
                <ReminderPanel
                    isOpen={isPanelOpen('reminder')}
                    onClose={closePanel}
                    player={reminderData.player}
                    addReminderClicked={(reminderId: string) => addReminderClicked?.(reminderData.player.id, reminderId)}
                />
            }
            {tokenData && (
                <TokenPanel
                    isOpen={isPanelOpen('token')}
                    onClose={closePanel}
                    player={tokenData.player}
                    isDraftMode={isDraftMode}
                    setRole={setRole}
                />
            )}
        </>
    );
}