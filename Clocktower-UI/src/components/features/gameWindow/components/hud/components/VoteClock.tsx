import type {CSSProperties} from "react";
import {
    AnimatePresence,
    motion
} from "framer-motion";
import {animations} from "@/constants";

interface VoteClockProps {
    bigHandRotation: number,
    smallHandRotation: number,
    clockSize: string,
    votingSpeed: number,
    countdown?: string
}

export const VoteClock = ({bigHandRotation, smallHandRotation, clockSize, votingSpeed, countdown}: VoteClockProps) => {
    return (
        <div className="clock-container">
            <img
                src="/assets/clock-big.png"
                className="clock-big"
                style={{
                    '--clock-rotation': `${bigHandRotation}deg`,
                    '--clock-transition-duration': `${votingSpeed - 100}ms`,
                    '--clock-size': clockSize
                } as CSSProperties}
                alt="Nominee clock hand"
            />
            <img
                src="/assets/clock-small.png"
                className="clock-small"
                style={{
                    '--clock-rotation': `${smallHandRotation}deg`,
                    '--clock-transition-duration': `${votingSpeed - 100}ms`,
                    '--clock-size': clockSize
                } as CSSProperties}
                alt="Nominator clock hand"
            />

            <AnimatePresence mode="wait">
                {countdown !== undefined && (
                    <motion.p key={countdown} className="countdown" {...animations.fade}>
                        {countdown}
                    </motion.p>
                )}
            </AnimatePresence>
        </div>
    );
}
