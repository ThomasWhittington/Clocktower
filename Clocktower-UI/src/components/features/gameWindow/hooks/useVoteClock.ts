import {useServerHub} from "@/hooks";
import {useDiscordTown} from "@/components/features/discordTownPanel/hooks";

export const useVoteClock = (circleDiameter: number) => {
    const {targetLock} = useServerHub();
    const {discordTown} = useDiscordTown();
    const voteActive = true;

    const bigHandMultiplier = 0.85;
    const votingSpeed = 2000;
    const rotation = 360;
    const playerCount = discordTown?.players.length ?? 0;
    const clockRotation = Math.round((rotation * (targetLock ?? 0)) / playerCount);
    const clockSize = circleDiameter > 0 ? `${circleDiameter * bigHandMultiplier}px` : '75vmin';

    return {
        voteActive,
        clockRotation,
        clockSize,
        votingSpeed
    }
}