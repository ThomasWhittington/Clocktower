import {
    useEffect,
    useRef,
} from 'react';
import {useAppStore} from "@/store";
import {
    type AudioEvent,
    AudioId
} from "@/types";

type AudioPlayerProps = {
    audioEvent?: AudioEvent;
};

const audioSources: Record<AudioId, string | null> = {
    [AudioId.Stop]: null,
    [AudioId.Countdown]: '/audio/countdown.mp3',
    [AudioId.Timer10Seconds]: 'audio/timer-10-seconds.mp3',
    [AudioId.TimerUp]: 'audio/timer-up.mp3',
    [AudioId.TimeToDay]: '/audio/time-to-day.mp3',
    [AudioId.TimeToEvening]: 'audio/time-to-evening.mp3',
    [AudioId.TimeToNight]: 'audio/time-to-night.mp3',
    [AudioId.RoleAssigned]: 'audio/role-assigned.mp3',
    [AudioId.Nomination]: 'audio/nomination.mp3',
    [AudioId.PlayerDead]: 'audio/player-dead.mp3',
    [AudioId.PlayerRevive]: 'audio/player-revive.mp3',
    [AudioId.HandPassUp]: 'audio/hand-pass-up.mp3',
    [AudioId.HandPassDown]: 'audio/hand-pass-down.mp3',
    [AudioId.TalkRequest]: 'audio/talk-request.mp3',
    [AudioId.NominationsOpen]: 'audio/nominations-open.mp3',
};
const AudioPlayer = ({audioEvent}: AudioPlayerProps) => {
    const {volume} = useAppStore();
    const audioRef = useRef<HTMLAudioElement>(null);

    useEffect(() => {
        if (audioRef.current) {
            audioRef.current.volume = volume;
            audioRef.current.muted = volume === 0;
        }
    }, [volume]);

    useEffect(() => {
        console.log('AudioPlayer useEffect triggered with audioEvent:', audioEvent?.audioId ? AudioId[audioEvent.audioId] : undefined);

        if (!audioEvent || !audioRef.current) return;

        const audio = audioRef.current;

        if (audioEvent.audioId === AudioId.Stop) {
            audio.pause();
            audio.currentTime = 0;
            audio.removeAttribute('src');
            audio.load();
            return;
        }

        const audioSource = audioSources[audioEvent.audioId];

        if (audioSource === null) {
            console.warn(`Audio id '${AudioId[audioEvent.audioId]}' has no playable source.`);
            return;
        }

        audio.src = audioSource;
        audio.currentTime = 0;
        audio.volume = volume;
        audio.muted = volume === 0;

        audio.play().catch((error) => {
            console.error(`Failed to play audio '${AudioId[audioEvent.audioId]}':`, error);
        });
    }, [audioEvent]);

    return <audio ref={audioRef}>
        <track kind="captions" src="/audio/empty.vtt" srcLang="en" label="No captions available"/>
    </audio>;
};

export default AudioPlayer;