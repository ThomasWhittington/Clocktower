import type {MouseEvent, ReactNode} from "react";

export function PlayerNameLabel({name, onClick, children}: Readonly<{
    name: string;
    onClick?: (e: MouseEvent) => void;
    children?: ReactNode;
}>) {
    return (

        <div className="player-name-container">
            <button onClick={onClick}>
                {name}
            </button>
            {children}
        </div>
    );
}