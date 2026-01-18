import {AnimatePresence, motion} from "framer-motion";
import type {User} from "@/types";
import {UserAvatar} from "@/components/ui";
import {Token} from "@/components/tokens";

const flipAnimation = {
    initial: {rotateY: -90, opacity: 0},
    animate: {rotateY: 0, opacity: 1},
    exit: {rotateY: 90, opacity: 0},
    transition: {duration: 0.3, ease: "easeInOut" as const},
    style: {willChange: "transform, opacity", perspective: 1000}
};

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
                <motion.div key="token" {...flipAnimation}>
                    <Token role={showDraftRoles ? player.draftRole : player.role} size={size} isDead={player.isDead} onClick={() => onTokenClick?.(player)}/>
                </motion.div>
            ) : (
                <motion.div key="avatar" {...flipAnimation}>
                    <UserAvatar user={player} size={size}/>
                </motion.div>
            )}
        </AnimatePresence>
    );
}