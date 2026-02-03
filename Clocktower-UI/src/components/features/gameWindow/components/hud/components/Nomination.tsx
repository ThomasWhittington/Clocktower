import {
    AnimatePresence,
    motion
} from "framer-motion";
import {animations} from "@/constants";
import {VoteClock} from "@/components/features/gameWindow/components/hud/components/VoteClock.tsx";

interface NominationProps {
    bigHandRotation: number,
    smallHandRotation: number,
    clockSize: string,
    votingSpeed: number,
    countdown?: string
}

export const Nomination = ({bigHandRotation, smallHandRotation, clockSize, votingSpeed, countdown}: NominationProps) => {
    return (
        <div className="nomination-container">
            <VoteClock bigHandRotation={bigHandRotation} smallHandRotation={smallHandRotation} clockSize={clockSize} votingSpeed={votingSpeed}/>

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
