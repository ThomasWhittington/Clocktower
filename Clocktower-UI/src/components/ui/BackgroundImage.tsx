import React from "react";
import {GameTime} from "@/types";

export const BackgroundImage = ({ gameTime, children }: { gameTime: GameTime, children?: React.ReactNode }) => {
    const backgrounds = {
        [GameTime.Unknown]: '/images/day-bg.png',
        [GameTime.Day]: '/images/day-bg.png',
        [GameTime.Evening]: '/images/evening-bg.png',
        [GameTime.Night]: '/images/night-bg.png'
    };
    
    return (
        <div className="relative flex flex-col w-full h-full min-h-0 overflow-hidden">
            {Object.entries(backgrounds).map(([time, image]) => (
                <div
                    key={time}
                    className={`absolute inset-0 bg-cover bg-center transition-opacity duration-1000 ease-in-out ${
                        Number.parseInt(time) === gameTime ? 'opacity-100' : 'opacity-0'
                    }`}
                    style={{ backgroundImage: `url("${image}")` }}
                />
            ))}

            <div className="absolute inset-0 backdrop-blur-sm" />

            <div className="relative z-10">
                {children}
            </div>
        </div>
    );
};