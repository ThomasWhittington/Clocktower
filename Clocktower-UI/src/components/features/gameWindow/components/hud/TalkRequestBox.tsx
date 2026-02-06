import {useAppStore} from "@/store";
import {useDiscordTown} from "@/components/features/discordTownPanel/hooks";
import {useMemo} from "react";
import {useServerHub} from "@/hooks";

export default function TalkRequestsBox() {
    const {currentUser} = useAppStore();
    const {talkRequests} = useServerHub();
    const {discordTown} = useDiscordTown();

    const displayRequests = useMemo(() => {
        if (!talkRequests || !discordTown?.gameUsers || !currentUser) return [];

        return talkRequests
            .filter(req => req.requesterId === currentUser.id || req.targetId === currentUser.id)
            .map(req => {
                const requester = discordTown.gameUsers.find(p => p.id === req.requesterId);
                const target = discordTown.gameUsers.find(p => p.id === req.targetId);
                const isOutgoing = req.requesterId === currentUser.id;

                return {
                    requesterName: requester?.name ?? "Unknown",
                    targetName: target?.name ?? "Unknown",
                    isOutgoing
                };
            });
    }, [talkRequests, discordTown?.gameUsers, currentUser]);

    if (displayRequests.length === 0) return null;

    return (
        <div className="talk-requests-box">
            <div className="talk-requests-header">
                💬 Talk Requests ({displayRequests.length})
            </div>
            <div className="talk-requests-list">
                {displayRequests.map((req) => (
                    <div key={`${req.requesterName}-${req.targetName}`} className="talk-request-item">
                        {
                            req.isOutgoing ? (
                                    <>
                                        <span className="you"> You </span>
                                        <span className="arrow">→</span>
                                        <span> {req.targetName} </span>
                                    </>
                                ) :
                                (
                                    <>
                                        <span>{req.requesterName} </span>
                                        <span className="arrow">→</span>
                                        <span className="you"> You </span>
                                    </>
                                )
                        }
                    </div>
                ))
                }
            </div>
        </div>
    );
}