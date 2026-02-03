import type {CSSProperties} from "react";

interface VoteClockProps {
    bigHandRotation: number,
    smallHandRotation: number,
    clockSize: string,
    votingSpeed: number,
}

export const VoteClock = ({bigHandRotation, smallHandRotation, clockSize, votingSpeed}: VoteClockProps) => {
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
        </div>
    );
}
