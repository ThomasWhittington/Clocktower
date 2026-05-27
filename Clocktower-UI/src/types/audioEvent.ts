export enum AudioId {
    Stop = 0,
    Countdown = 1,
    TimerUp = 2,
    TimeToDay = 3,
    TimeToEvening = 4,
    TimeToNight = 5,
    RoleAssigned = 6,
    Nomination = 7,
    PlayerDead = 8,
    PlayerRevive = 9,
    HandPassUp = 10,
    HandPassDown = 11,
    TalkRequest = 12
}

export type AudioEvent = {
    id: string;
    audioId: AudioId;
};