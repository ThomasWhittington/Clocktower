import {
    useEffect,
    useRef,
    useState
} from 'react';
import {useServerHub} from "@/hooks";
import {useNominationState} from "@/components/features/gameWindow/hooks/useNominationState.ts";

export const useVoteClock = (circleDiameter: number) => {
    const {nominationSession} = useServerHub();
    const {
        nominationsEnabled,
        isActiveNomination,
        voteUnderway,
        toggleNominations
    } = useNominationState();

    const nominator = nominationSession?.nominator ?? 0;
    const target = nominationSession?.currentTarget ?? 0;

    const lastTargetRef = useRef<number | undefined>(undefined);
    const rotationCountRef = useRef(0);
    const [bigHandRotation, setBigHandRotation] = useState(0);
    const [smallHandRotation, setSmallHandRotation] = useState(0);

    const bigHandMultiplier = 0.85;
    const votingSpeed = nominationSession?.votingSpeed ?? 0;
    const playerCount = nominationSession?.playerCount ?? 0;
    useEffect(() => {
        if (target !== undefined && playerCount > 0) {
            const lastTarget = lastTargetRef.current;

            if (lastTarget !== undefined && lastTarget > target) {
                rotationCountRef.current++;
            }

            lastTargetRef.current = target;

            const degreesPerPlayer = 360 / playerCount;
            const baseRotation = Math.round(degreesPerPlayer * target);

            const totalRotation = baseRotation + (rotationCountRef.current * 360);
            setBigHandRotation(totalRotation);
            const nominatorRotation = Math.round(degreesPerPlayer * nominator);
            setSmallHandRotation(nominatorRotation);
        } else {
            lastTargetRef.current = undefined;
            rotationCountRef.current = 0;
            setBigHandRotation(0);
            setSmallHandRotation(0);
        }
    }, [nominationSession, target, nominator]);

    const clockSize = circleDiameter > 0 ? `${circleDiameter * bigHandMultiplier}px` : '75vmin';
    const countdown = nominationSession?.countDownString;

    return {
        nominationsEnabled,
        voteUnderway,
        bigHandRotation,
        smallHandRotation,
        clockSize,
        votingSpeed,
        countdown,
        isActiveNomination,
        toggleNominations
    }
}