import type {CSSProperties} from "react";

interface VoteClockProps {
    clockRotation: number,
    clockSize: string,
    votingSpeed: number
}

export const VoteClock = ({clockRotation, clockSize, votingSpeed}: VoteClockProps) => {


    return (
        <img
            src="/assets/clock-big.png"
            className="clock-big"
            style={{
                '--clock-rotation': `${clockRotation}deg`,
                '--clock-transition-duration': `${votingSpeed}ms`,
                '--clock-size': clockSize
            } as CSSProperties}
            alt="Nominee clock hand"
        />
    );
}
