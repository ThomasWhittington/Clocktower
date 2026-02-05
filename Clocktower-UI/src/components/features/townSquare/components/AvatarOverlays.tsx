import {
    Shroud,
    VoteIcon
} from "@/components/ui/icons";
import {DiscordUserVoiceStatus} from "@/components/ui";
import type {User} from "@/types";
import {useCurrentUserIsStoryteller} from "@/components/features/discordTownPanel/hooks";
import {usePlayerBadgeActions} from "@/components/features/townSquare/hooks";

interface AvatarOverlaysProps {
    player: User;
}

export function AvatarOverlays({player}: Readonly<AvatarOverlaysProps>) {
    const isStoryteller = useCurrentUserIsStoryteller();
    const {handleShroudClick, handleVoteTokenClick} = usePlayerBadgeActions(player);

    return (
        <>
            <button
                className={`shroud-overlay ${player.isDead ? 'is-dead' : 'is-alive'} ${isStoryteller ? 'is-interactive' : 'is-static'}`}
                onClick={handleShroudClick}
                disabled={!isStoryteller}
                aria-label={`${player.isDead ? 'Dead' : 'Alive'} - ${player.name}`}
            >
                <Shroud/>
            </button>
            {player.isDead && player.hasVoteToken && (
                <button
                    className={`vote-token-badge ${isStoryteller ? 'is-interactive' : 'is-static'}`}
                    onClick={handleVoteTokenClick}
                    disabled={!isStoryteller}
                    aria-label={`Vote token - ${player.name}`}
                >
                    <VoteIcon/>
                </button>
            )}

            {player.voiceState && (
                <div className="voice-status-badge">
                    <DiscordUserVoiceStatus voiceState={player.voiceState}/>
                </div>
            )}
        </>
    );
}