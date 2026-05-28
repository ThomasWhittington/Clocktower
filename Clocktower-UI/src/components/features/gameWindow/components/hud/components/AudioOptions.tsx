import {
    useRef,
    useState
} from 'react';
import {IconButton} from "@/components/ui";
import {
    AudioMuted,
    AudioUnMuted
} from "@/components/ui/icons";
import {useAppStore} from "@/store";

export const AudioOptions = () => {
    const {volume, setVolume} = useAppStore();

    const isMuted = volume === 0;
    const [previousVolume, setPreviousVolume] = useState(1);
    const [isSliderHovered, setIsSliderHovered] = useState(false);
    const sliderRef = useRef<HTMLInputElement>(null);

    const toggleMute = () => {
        if (isMuted) {
            setVolume(previousVolume);
        } else {
            setPreviousVolume(volume);
            setVolume(0);
        }
    };

    return (
        <div className="audio-options">
            <IconButton icon={isMuted ? <AudioMuted/> : <AudioUnMuted/>} onClick={toggleMute} tooltip={isMuted ? "Unmute" : "Mute"}/>
            <div className="slider">
                <input
                    ref={sliderRef}
                    type="range"
                    min="0"
                    max="1"
                    step="0.01"
                    value={volume}
                    onChange={(e) => {
                        const next = Number.parseFloat(e.target.value);
                        if (next > 0) setPreviousVolume(next);
                        setVolume(next);
                    }}
                    onMouseEnter={() => setIsSliderHovered(true)}
                    onMouseLeave={() => setIsSliderHovered(false)}
                />
                {isSliderHovered && (
                    <div
                        className="tooltip"
                        style={{
                            left: `${volume * 100}%`,
                            transform: 'translateX(-50%)'
                        }}
                    >
                        {Math.round(volume * 100)}%
                    </div>
                )}
            </div>
        </div>
    );
};

export default AudioOptions;