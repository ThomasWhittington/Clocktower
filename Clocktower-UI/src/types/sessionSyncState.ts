import {
    DiscordTown,
    type GameTime,
    NominationSession,
    Script,
    type TimerState
} from "@/types";

export type SessionSyncState = {
    gameTime: GameTime,
    jwt: string,
    discordTown?: DiscordTown;
    timer?: TimerState;
    script?: Script;
    nominationSession?: NominationSession
};