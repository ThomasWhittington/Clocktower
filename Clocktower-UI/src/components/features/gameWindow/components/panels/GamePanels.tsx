import {
    NightOrderPanel,
    RoleListPanel,
    RolePlannerPanel,
    ScriptManagerPanel,
    TokenPanel,
    UserManagerPanel
} from "./index";
import type {PanelType} from "@/components/features/gameWindow/hooks";
import {Role} from "@/types";
import type {
    Dispatch,
    SetStateAction
} from "react";

interface GamePanelsProps {
    isPanelOpen: (panel: PanelType) => boolean;
    closePanel: () => void;
    tokenData: any;
    isDraftMode: boolean;
    setRole: any;
    setIsDraftMode: (callback: (prev: boolean) => boolean) => void;
    selectedRoles: Role[];
    setSelectedRoles: Dispatch<SetStateAction<Role[]>>;
}

export function GamePanels({isPanelOpen, closePanel, tokenData, isDraftMode, setRole, setIsDraftMode, setSelectedRoles, selectedRoles}: Readonly<GamePanelsProps>) {
    return (
        <>
            <UserManagerPanel isOpen={isPanelOpen('user')} onClose={closePanel}/>
            <ScriptManagerPanel isOpen={isPanelOpen('script')} onClose={closePanel}/>
            <RoleListPanel isOpen={isPanelOpen('role')} onClose={closePanel}/>
            <NightOrderPanel isOpen={isPanelOpen('night')} onClose={closePanel}/>
            <RolePlannerPanel isOpen={isPanelOpen('rolePlanner')} onClose={closePanel} setIsDraftMode={setIsDraftMode} setSelectedRoles={setSelectedRoles} selectedRoles={selectedRoles}/>

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