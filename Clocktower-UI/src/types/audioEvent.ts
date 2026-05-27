export enum AudioId {
    Stop = 0,
    Countdown = 1,
    Timer10Seconds = 2,
    TimerUp = 3,
    TimeToDay = 4,
    TimeToEvening = 5,
    TimeToNight = 6,
    RoleAssigned = 7,
    Nomination = 8,
    PlayerDead = 9,
    PlayerRevive = 10,
    HandPassUp = 11,
    HandPassDown = 12,
    TalkRequest = 13
}

export type AudioEvent = {
    id: string;
    audioId: AudioId;
};