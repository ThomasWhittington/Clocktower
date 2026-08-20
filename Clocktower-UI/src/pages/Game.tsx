import {
    Panel,
    PanelGroup,
    PanelResizeHandle
} from "react-resizable-panels";
import {
    DiscordTownPanel,
    GameWindow
} from "../components/features";
import {useServerHub} from "@/hooks";
import {BackgroundImage} from "@/components/ui";
import AudioPlayer from "@/components/audio/AudioPlayer.tsx";
import {useAppStore} from "@/store";
import {useEffect} from "react";

function Game() {
    useEffect(() => {
        const urlParams = new URLSearchParams(globalThis.location.search);
        const gameId = urlParams.get('gameId');
        if (gameId) {
            useAppStore.getState().setGameId(gameId);
            globalThis.location.href = '/game';
        }
    }, []);

    const {gameTime, audioEvent} = useServerHub();

    return (
        <>
            <AudioPlayer audioEvent={audioEvent}/>
            <PanelGroup autoSaveId="game-panel-layout" direction="horizontal">
                <Panel defaultSize={20} collapsible={true} minSize={20}>
                    <DiscordTownPanel/>
                </Panel>
                <PanelResizeHandle className="w-2 bg-gray-400 hover:bg-gray-600 cursor-col-resize"/>
                <Panel className="flex justify-center align-center">
                    <BackgroundImage gameTime={gameTime}>
                        <GameWindow/>
                    </BackgroundImage>
                </Panel>
            </PanelGroup>
        </>
    );
}

export default Game;