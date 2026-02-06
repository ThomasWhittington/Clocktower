import {VoteHistoryRecord} from "@/types";
import {useDiscordTown} from "@/components/features/discordTownPanel/hooks";

export const VoteHistoryLine = ({voteHistory}: { voteHistory: VoteHistoryRecord }) => {
    const {discordTown} = useDiscordTown();

    const getUserName = (userId: string): string => {
        const user = discordTown?.gameUsers?.find(u => u.id === userId);
        return user ? user.name : userId;
    };

    const formattedTime = voteHistory.dateTime.toLocaleTimeString();
    const nominatorName = getUserName(voteHistory.nominatorId);
    const nomineeName = getUserName(voteHistory.nomineeId);
    const voterNames = voteHistory.voters.map(voterId => getUserName(voterId)).join(", ");
    const passed = voteHistory.voteCount >= voteHistory.requiredMajority;

    return (
        <tr className={`vote-history-line ${passed ? 'passed' : 'failed'}`}>
            <td className="time">{formattedTime}</td>
            <td className="nominator">{nominatorName}</td>
            <td className="nominee">{nomineeName}</td>
            <td className="votes">{voteHistory.voteCount}/{voteHistory.requiredMajority}</td>
            <td className="result">{passed ? '✓' : '✗'}</td>
            <td className="voters">{voterNames || 'None'}</td>
        </tr>
    );
}