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
import {animations} from "@/constants";

interface CenterHudProps {
    circleDiameter: number;
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
                    <motion.div key="nominations" {...animations.zoomIn}>
                        <VoteClock
                            bigHandRotation={bigHandRotation}
                            smallHandRotation={smallHandRotation}
                            clockSize={clockSize}
                            votingSpeed={votingSpeed}
                            countdown={countdown}
                        />
                    </motion.div>
                ) : (
                    <motion.div key="default" {...animations.zoomIn}>
                        <Timer/>
                        <RoleDistributionWidget/>
                    </motion.div>
                )}
            </AnimatePresence>
        </div>
    );
}
