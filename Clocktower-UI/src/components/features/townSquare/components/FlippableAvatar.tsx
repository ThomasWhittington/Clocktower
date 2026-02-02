import {
    AnimatePresence,
    motion
} from "framer-motion";
import type {User} from "@/types";
import {UserAvatar} from "@/components/ui";
import {Token} from "@/components/tokens";
import {animations} from "@/constants";

interface FlippableAvatarProps {
    player: User;
    size: number;
    showToken: boolean;
    showDraftRoles: boolean;
    onTokenClick?: (player: User) => void;
}

export function FlippableAvatar({player, size, showToken, showDraftRoles, onTokenClick}: Readonly<FlippableAvatarProps>) {
    return (
        <AnimatePresence mode="wait">
            {showToken ? (
                <motion.div key="token" {...animations.flip}>
                    <Token role={showDraftRoles ? player.draftRole : player.role} size={size} isDead={player.isDead} onClick={() => onTokenClick?.(player)}/>
                </motion.div>
            ) : (
                <motion.div key="avatar" {...animations.flip}>
                    <UserAvatar user={player} size={size}/>
                </motion.div>
            )}
        </AnimatePresence>
    );
}