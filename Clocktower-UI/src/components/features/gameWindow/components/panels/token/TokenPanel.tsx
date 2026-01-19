import {BasePanel} from "@/components/ui";
import {
    Role,
    type User
} from "@/types";
import {
    useElementSize,
    useServerHub
} from "@/hooks";
import {TokenGroup} from "@/components/features/gameWindow/components";
import {Token} from "@/components/tokens";
import {useAppStore} from "@/store";
import {useUser} from "@/components/features/discordTownPanel/hooks";
import {UserUtils} from "@/utils";

interface TokenPanelProps {
    isOpen: boolean,
    onClose: () => void,
    player: User,
    isDraftMode: boolean,
    setRole: (role: (Role | undefined), targetUserId: string) => Promise<void>
}

export const TokenPanel = ({isOpen, onClose, player, isDraftMode, setRole}: TokenPanelProps) => {
    const currentUser = useAppStore((state) => state.currentUser);
    const {thisUser} = useUser(currentUser?.id);
    const {script} = useServerHub();
    const {ref: containerRef, size: parentSize} = useElementSize<HTMLDivElement>();
    if (!player || !currentUser) return null;

    const playerRole = isDraftMode ? player.draftRole : player.role;

    const tokenClicked = async (role: Role | undefined) => {
        await setRole(role, player.id);
        onClose();
    }

    const dynamicSize = Math.min(parentSize.width, parentSize.height) / 8;
    return (
        <BasePanel title={`Choose a new character for ${player.name}`} isOpen={isOpen} onClose={onClose}>
            <div ref={containerRef} className="token-panel">
                <TokenGroup name="Townsfolk" roles={script?.townsfolk} tokenSize={dynamicSize} currentRoleId={playerRole?.id} onClick={tokenClicked}/>

                <div className="outsider-minion-demon-group">
                    <TokenGroup name="Outsiders" roles={script?.outsiders} tokenSize={dynamicSize} currentRoleId={playerRole?.id} onClick={tokenClicked}/>
                    <TokenGroup name="Minions" roles={script?.minions} tokenSize={dynamicSize} currentRoleId={playerRole?.id} onClick={tokenClicked}/>
                    <TokenGroup name="Demons" roles={script?.demons} tokenSize={dynamicSize} currentRoleId={playerRole?.id} onClick={tokenClicked}/>
                </div>

                <div className="no-role-token">
                    <Token
                        size={dynamicSize * 1.5}
                        key="no-role"
                        customName="Remove Role"
                        className={playerRole?.id === undefined ? 'current-role' : undefined}
                        onClick={() => tokenClicked(undefined)}
                    />
                </div>
                {UserUtils.isStoryTeller(thisUser) &&
                    <TokenGroup name="Travellers" roles={script?.travellers} tokenSize={dynamicSize} currentRoleId={playerRole?.id} onClick={tokenClicked}/>
                }
            </div>
        </BasePanel>
    )
};