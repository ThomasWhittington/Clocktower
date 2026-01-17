import {BasePanel} from "@/components/ui";
import type {User} from "@/types";

interface TokenPanelProps {
    isOpen: boolean;
    onClose: () => void;
    player: User;
}

export const TokenPanel = ({isOpen, onClose, player}: TokenPanelProps) => {
    if (!player) return null;

    return (
        <BasePanel title={`${player.name}'s Token`} isOpen={isOpen} onClose={onClose}>
            <div>
                <p>Player: {player.name}</p>
                <p>Role: {player.role?.name}</p>
            </div>
        </BasePanel>
    );
};