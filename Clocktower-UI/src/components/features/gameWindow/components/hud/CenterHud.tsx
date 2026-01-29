import {useVoteClock} from "@/components/features/gameWindow/hooks";
import {
    RoleDistributionWidget,
    Timer
} from "@/components/ui";
import {VoteClock} from "@/components/features/gameWindow/components";

interface CenterHudProps {
    circleDiameter: number;
}

export const CenterHud = ({circleDiameter}: CenterHudProps) => {
    const {
        voteActive,
        clockRotation,
        clockSize,
        votingSpeed
    } = useVoteClock(circleDiameter);


    return (
        <div className="controls-center">
            {voteActive ?
                <VoteClock
                    clockRotation={clockRotation}
                    clockSize={clockSize}
                    votingSpeed={votingSpeed}
                />
                :
                <>
                    <Timer/>
                    <RoleDistributionWidget/>
                </>
            }
        </div>
    );
}
