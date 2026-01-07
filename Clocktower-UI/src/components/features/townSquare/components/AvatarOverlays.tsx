import {Shroud, VoteToken} from "@/components/ui/icons";
import {DiscordUserVoiceStatus} from "@/components/ui";
import type {User} from "@/types";
import {UserUtils} from "@/utils";
import {useAppStore} from "@/store";
import {useUser} from "@/components/features/discordTownPanel/hooks";
import {usePlayerBadgeActions} from "@/components/features/townSquare/hooks";

interface AvatarOverlaysProps {
    player: User;
}

export function AvatarOverlays({player}: Readonly<AvatarOverlaysProps>) {
    const {currentUser} = useAppStore();
    const {thisUser} = useUser(currentUser?.id);
    const isStoryteller = UserUtils.isStoryTeller(thisUser);
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
                    <VoteToken/>
                </button>
            )}

            {player.voiceState && (
                <div className="voice-status-badge">
                    <DiscordUserVoiceStatus voiceState={player.voiceState} iconSize={35}/>
                </div>
            )}
        </>
    );
}