import {BasePanel} from "@/components/ui";
import {
    useEffect,
    useState
} from "react";
import {useAppStore} from "@/store";
import {VoteHistoryRecord} from "@/types";
import {getVoteHistory} from "@/hooks";
import {VoteHistoryLine} from "@/components/features/gameWindow/components/panels/voteHistory/VoteHistoryLine.tsx";

interface VoteHistoryPanelProps {
    isOpen: boolean;
    onClose: () => void;
}

export const VoteHistoryPanel = ({isOpen, onClose}: VoteHistoryPanelProps) => {
    const {gameId} = useAppStore();
    const [voteHistory, setVoteHistory] = useState<VoteHistoryRecord[]>([]);
    const [isLoading, setIsLoading] = useState(false);
    useEffect(() => {
        if (isOpen && gameId) {
            setIsLoading(true);
            getVoteHistory(gameId)
                .then(history => {
                    setVoteHistory(history ?? []);
                })
                .catch(error => {
                    console.error('Failed to fetch vote history:', error);
                    setVoteHistory([]);
                })
                .finally(() => {
                    setIsLoading(false);
                });
        }
    }, [isOpen, gameId]);
    console.log(voteHistory)
    return (
        <BasePanel title="Vote History" isOpen={isOpen} onClose={onClose}>
            <div className="vote-history-panel">
                {isLoading && <p>Loading vote history...</p>}
                {!isLoading && voteHistory.length === 0 && <p>No vote history available.</p>}
                {!isLoading && voteHistory.length > 0 && (
                    <div>
                        <table>
                            <thead>
                            <tr>
                                <th>Time</th>
                                <th>Nominator</th>
                                <th>Nominee</th>
                                <th>Votes</th>
                                <th>Result</th>
                                <th>Voters</th>
                            </tr>
                            </thead>
                            <tbody>
                            {voteHistory.sort((a, b) => b.dateTime.getTime() - a.dateTime.getTime()).map((record) => (
                                <VoteHistoryLine voteHistory={record} key={`${record.nominatorId}-${record.nomineeId}}`}/>
                            ))}
                            </tbody>
                        </table>
                    </div>
                )}
            </div>
        </BasePanel>
    );
};
