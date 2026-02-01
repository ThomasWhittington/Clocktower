import {useVoteClock} from "@/components/features/gameWindow/hooks";
import {
    IconButton,
    RoleDistributionWidget,
    Timer
} from "@/components/ui";
import {VoteClock} from "@/components/features/gameWindow/components";
import {
    AnimatePresence,
    motion
} from "framer-motion";
import {VoteIcon} from "@/components/ui/icons";
import {useCurrentUserIsStoryteller} from "@/components/features/discordTownPanel/hooks";

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
        nominationsEnabled,
        bigHandRotation,
        smallHandRotation,
        clockSize,
        votingSpeed,
        countdown,
        isActiveNomination,
        toggleNominations
    } = useVoteClock(circleDiameter);
    const isStoryteller = useCurrentUserIsStoryteller();
    const openNominationsButtonText = nominationsEnabled ? 'Close Nominations' : 'Open Nominations';

    return (
        <div className="controls-center">
            {isStoryteller &&
                <IconButton icon={<VoteIcon/>} text={openNominationsButtonText} variant={nominationsEnabled ? 'danger' : 'primary'} className="cursor-pointer pointer-events-auto" onClick={toggleNominations}/>
            }
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
