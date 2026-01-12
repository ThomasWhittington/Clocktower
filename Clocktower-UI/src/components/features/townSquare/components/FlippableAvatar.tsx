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

export function FlippableAvatar({player, size, showToken}: Readonly<{ player: User; size: number; showToken: boolean; }>) {
    return (
        <AnimatePresence mode="wait">
            {showToken ? (
                <motion.div key="token" {...flipAnimation}>
                    <Token player={player} size={size}/>
                </motion.div>
            ) : (
                <motion.div key="avatar" {...flipAnimation}>
                    <UserAvatar user={player} size={size}/>
                </motion.div>
            )}
        </AnimatePresence>
    );
}