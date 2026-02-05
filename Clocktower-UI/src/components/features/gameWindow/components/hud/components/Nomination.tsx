import {
    AnimatePresence,
    motion
} from "framer-motion";
import {animations} from "@/constants";
import {
    VoteClock,
    VoteOverlay
} from "@/components/features/gameWindow/components/hud/components";
import {useVoteClock} from "@/components/features/gameWindow/hooks";

interface NominationProps {
    circleDiameter: number;
}

export const Nomination = ({circleDiameter}: NominationProps) => {
    const {
        bigHandRotation,
        smallHandRotation,
        clockSize,
        votingSpeed,
        countdown
    } = useVoteClock(circleDiameter);
    return (
        <div className="nomination-container">
            <VoteClock bigHandRotation={bigHandRotation} smallHandRotation={smallHandRotation} clockSize={clockSize} votingSpeed={votingSpeed}/>

            <AnimatePresence>
                {countdown !== undefined && (
                    <motion.p key={countdown} className="countdown" {...animations.fade}>
                        {countdown}
                    </motion.p>
                )}
                <motion.div key="vote-overlay" {...animations.fade}>
                    <VoteOverlay/>
                </motion.div>
            </AnimatePresence>


        </div>
    );
}
