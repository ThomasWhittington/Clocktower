export class VoteHistoryRecord {
    readonly time: string;
    readonly nominatorId: string;
    readonly nomineeId: string;
    readonly voteCount: number;
    readonly requiredMajority: number;
    readonly voters: string[];

    constructor(record: VoteHistoryRecord) {
        this.time = record.time;
        this.nominatorId = record.nominatorId;
        this.nomineeId = record.nomineeId;
        this.voteCount = record.voteCount;
        this.requiredMajority = record.requiredMajority;
        this.voters = record.voters;
    }

    public get dateTime(): Date {
        return new Date(this.time);
    }
}