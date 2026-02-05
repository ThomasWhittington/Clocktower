export class NominationSession {
    readonly gameId: string;
    readonly nominator?: number;
    readonly nominee?: number;
    readonly voteUnderway?: boolean;
    readonly voteEnded?: boolean;
    readonly currentTarget?: number;
    readonly votingSpeed?: number;
    readonly playerCount: number;
    readonly requiredMajority?: number;
    readonly countDown?: number;

    constructor(session: NominationSession) {
        this.gameId = session.gameId;
        this.nominator = session.nominator;
        this.nominee = session.nominee;
        this.voteUnderway = session.voteUnderway;
        this.voteEnded = session.voteEnded;
        this.currentTarget = session.currentTarget;
        this.votingSpeed = session.votingSpeed;
        this.playerCount = session.playerCount;
        this.requiredMajority = session.requiredMajority;
        this.countDown = session.countDown;
    }

    get countDownString(): string | undefined {
        if (this.countDown == undefined) return undefined;
        return this.countDown.toString() === "0" ? "GO" : this.countDown.toString();
    }

    get isActiveNomination(): boolean {
        return this.nominee !== null && this.nominator !== null;
    }
}