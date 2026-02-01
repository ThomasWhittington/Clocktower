export class NominationSession {
    readonly gameId: string;
    readonly nominator?: number;
    readonly nominee?: number;
    readonly voteUnderway?: boolean;
    readonly currentTarget?: number;
    readonly votingSpeed?: number;
    readonly playerCount: number;
    readonly countDown?: number;

    constructor(session: NominationSession) {
        this.gameId = session.gameId;
        this.nominator = session.nominator;
        this.nominee = session.nominee;
        this.voteUnderway = session.voteUnderway;
        this.currentTarget = session.currentTarget;
        this.votingSpeed = session.votingSpeed;
        this.playerCount = session.playerCount;
        this.countDown = session.countDown;
    }

    get countDownString(): string | undefined {
        if (!this.countDown) return undefined;
        return this.countDown.toString() === "0" ? "GO" : this.countDown.toString();
    }

    get isActiveNomination(): boolean {
        return this.nominee !== null && this.nominator !== null;
    }
}