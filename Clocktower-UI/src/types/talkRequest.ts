export class TalkRequest {
    readonly requesterId: string;
    readonly targetId: string;
    readonly timestamp: string;

    constructor(state: Partial<TalkRequest>) {
        this.requesterId = state.requesterId ?? '';
        this.targetId = state.targetId ?? '';
        this.timestamp = state.timestamp ?? '';
    }

    public get dateTime(): Date {
        return new Date(this.timestamp);
    }
}