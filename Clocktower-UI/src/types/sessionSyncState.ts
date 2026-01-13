import {DiscordTown, type GameTime, Script, type TimerState} from "@/types";

export type SessionSyncState = {
    gameTime: GameTime,
    jwt: string,
    discordTown?: DiscordTown;
    timer?: TimerState;
    script?: Script;
};