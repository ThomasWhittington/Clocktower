import {useVoteClock} from "@/components/features/gameWindow/hooks";
import {
    RoleDistributionWidget,
    Timer
} from "@/components/ui";
import {
    AnimatePresence,
    motion
} from "framer-motion";
import {VoteClock} from "@/components/features/gameWindow/components/hud/components";

interface CenterHudProps {
    circleDiameter: number;
}

const fadeAnimation = {
    initial: {opacity: 0},
    animate: {opacity: 1},
    exit: {opacity: 0},
    transition: {duration: 0.8}
}
export const CenterHud = ({circleDiameter}: CenterHudProps) => {
    const {
        bigHandRotation,
        smallHandRotation,
        clockSize,
        votingSpeed,
        countdown,
        isActiveNomination
    } = useVoteClock(circleDiameter);
  
    return (
        <div className="controls-center">
            <AnimatePresence mode="wait">
                {isActiveNomination ? (
                    <motion.div key="nominations" {...fadeAnimation}>
                        <VoteClock
                            bigHandRotation={bigHandRotation}
                            smallHandRotation={smallHandRotation}
                            clockSize={clockSize}
                            votingSpeed={votingSpeed}
                            countdown={countdown}
                        />
                    </motion.div>
                ) : (
                    <motion.div key="default" {...fadeAnimation}>
                        <Timer/>
                        <RoleDistributionWidget/>
                    </motion.div>
                )}
            </AnimatePresence>
        </div>
    );
}
