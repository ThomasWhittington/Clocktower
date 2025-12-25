import {DiscordTown, type GameTime, type TimerState} from "@/types";

export type SessionSyncState = {
    gameTime: GameTime,
    jwt: string,
    discordTown?: DiscordTown;
    timer?: TimerState;
};