import type {TimerStatus} from "@/types";

export type TimerState = {
    gameId: string;
    status: TimerStatus;
    serverNowUtc: string;
    endUtc?: string | null;
    label?: string | null;
};