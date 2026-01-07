import type {MouseEvent, ReactNode} from "react";
import {Edition, RoleType, type User} from "@/types";
import {DiscordUserVoiceStatus} from "@/components/ui";

export function PlayerNameLabel({player, onClick, children}: Readonly<{
    player: User;
    onClick?: (e: MouseEvent) => void;
    children?: ReactNode;
}>) {
    return (
        <div className="player-name-container">
            <button onClick={onClick}>
                <span className="flex items-center justify-center gap-1">
                    {player.name} {player.voiceState && <DiscordUserVoiceStatus voiceState={player.voiceState}/>}
                </span>
            </button>
            {player.role && <span className="text-xs">{player.role.name} {player.role.description.slice(0, 10)} {RoleType[player.role.type]} {Edition[player.role.edition]}</span>}
            <span className="text-xs">{`Dead: ${player.isDead}`}</span>
            <span className="text-xs">{`Marked: ${player.isMarked}`}</span>
            <span className="text-xs">{`VoteToken: ${player.hasVoteToken}`}</span>
            {children}
        </div>
    );
}