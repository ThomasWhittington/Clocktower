import {
    HandIcon,
    MinusIcon,
    PlusIcon,
    PointIcon,
    SkullIcon,
    VoteIcon
} from "@/components/ui/icons";
import {IconButton} from "@/components/ui";
import type {CSSProperties} from "react";
import {useVoteOverlay} from "@/components/features/gameWindow/hooks";
import {useKeyboardShortcut} from "@/hooks";

export const VoteOverlay = () => {
    const {
        voteUnderway,
        requiredMajority,
        canToggleVoteRunning,
        canUseVoteEndedControls,
        canVote,
        callToggleMarkPlayer,
        toggleVoteRunning,
        toggleHandUp,
        closeNominations,
        nextNomination,
        nomineeName,
        nominatorName,
        canRun,
        currentVoteCount,
        userHandUp,
        userMarked,
        setVoteSpeed,
        voteSpeed
    } = useVoteOverlay();

    useKeyboardShortcut({
        key: 'h',
        onKeyPress: toggleHandUp,
        enabled: canVote
    });


    if (!canRun) return null;

    const toggleStartVoteText = voteUnderway ? "Cancel" : "Start";
    const toggleVoteText = userHandUp ? "Lower [H]" : "Raise [H]";
    const toggleMarkedText = userMarked ? "Unmark" : "Mark";
    const buttonVariant = (toggle: boolean): "danger" | "primary" | "secondary" | "outline" | undefined => toggle ? "danger" : "primary";

    return (
        <div className="vote-overlay">
            <p className="vote-text"><span>{nominatorName}</span><PointIcon/><span>{nomineeName}</span></p>
            <p className="majority">(Majority: {requiredMajority})</p>
            {canToggleVoteRunning &&
                <div className="speed-control">
                    <IconButton
                        className="speed-button"
                        icon={<MinusIcon/>}
                        variant='secondary'
                        tooltip="Decrease speed"
                        onClick={() => setVoteSpeed(Math.max(500, voteSpeed - 500))}
                    />
                    <span className="speed-display">{(voteSpeed / 1000).toFixed(1)} seconds</span>
                    <IconButton
                        className="speed-button"
                        icon={<PlusIcon/>}
                        variant='secondary'
                        tooltip="Increase speed"
                        onClick={() => setVoteSpeed(Math.min(5000, voteSpeed + 500))}
                    />
                </div>
            }
            <div className="vote-control">
                <div className="current-count">
                    <HandIcon className={`hand${currentVoteCount >= requiredMajority ? ' majority' : ''}`} style={{'--gradient-end': 'transparent'} as CSSProperties}/>
                    <span className="count">{currentVoteCount}</span>
                </div>

                {canVote &&
                    <IconButton
                        className="toggle-vote"
                        icon={<HandIcon gradientId="shared-hand"/>}
                        text={toggleVoteText}
                        variant={buttonVariant(userHandUp)}
                        tooltip={toggleVoteText}
                        onClick={toggleHandUp}
                    />
                }
                {canToggleVoteRunning &&
                    <>
                        <IconButton
                            className="start-vote"
                            icon={<VoteIcon/>}
                            text={toggleStartVoteText}
                            variant={buttonVariant(voteUnderway)}
                            tooltip={toggleStartVoteText}
                            onClick={toggleVoteRunning}
                        />
                        <IconButton
                            className="next-nomination"
                            icon={<VoteIcon/>}
                            text="Next"
                            variant='secondary'
                            tooltip="Next"
                            onClick={nextNomination}
                        />
                    </>
                }
                {canUseVoteEndedControls &&
                    <>
                        <IconButton
                            className="toggle-mark"
                            icon={<SkullIcon/>}
                            text={toggleMarkedText}
                            variant={buttonVariant(userMarked)}
                            tooltip={toggleMarkedText}
                            onClick={callToggleMarkPlayer}
                        />
                        <IconButton
                            className="next-nomination"
                            icon={<VoteIcon/>}
                            text="Next"
                            variant='secondary'
                            tooltip="Next"
                            onClick={nextNomination}
                        />
                        <IconButton
                            className="close-nominations"
                            icon={<HandIcon/>}
                            text="Close"
                            variant='danger'
                            tooltip="Close"
                            onClick={closeNominations}
                        />
                    </>
                }
            </div>
        </div>
    )
        ;
}
