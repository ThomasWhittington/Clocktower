import {
    type PlayerActionContext,
    playerActions
} from '../config';
import type {User} from "@/types";
import {useNominationState} from "@/components/features/gameWindow/hooks";
import {useCurrentUserIsStoryteller} from "@/components/features/discordTownPanel/hooks";

interface PlayerActionMenuProps {
    player: User;
    context: PlayerActionContext;
}

export function PlayerActionMenu({player, context}: Readonly<PlayerActionMenuProps>) {
    const currentUserIsStoryTeller = useCurrentUserIsStoryteller();
    const {nominationsEnabled} = useNominationState();
    const visibleActions = playerActions.filter((action) =>
        action.isVisible(player, {nominationsEnabled, currentUserIsStoryTeller})
    );

    if (visibleActions.length === 0) return null;

    return (
        <div className="player-action-menu" role="menu" aria-label={`Actions for ${player.name}`} onPointerDown={(e) => e.stopPropagation()}>
            <div className="player-action-menu-header">
                <p className="descriptor">Player Actions</p>
                <p className="title">{player.name}</p>
            </div>
            <div className="player-action-menu-items">
                {visibleActions.map((action) => (
                    <button
                        key={action.id}
                        role="menuitem"
                        className="player-action-menu-item"
                        onClick={() => action.execute(player, context)}
                    >
                        <span>{action.icon}</span> {action.label}
                    </button>
                ))}
            </div>
        </div>
    );
}