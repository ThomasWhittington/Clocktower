import {
    Panel,
    PanelGroup,
    PanelResizeHandle
} from "react-resizable-panels";
import {
    DiscordTownPanel
} from "../components/features";
import {
    useDiscordHub
} from "@/hooks";
import {
    HubConnectionState
} from "@microsoft/signalr";
import {
    BackgroundImage,
    Spinner
} from "@/components/ui";

function Game() {
    const {
        connectionState,
        gameTime
    } = useDiscordHub();

    return (
        <>
            {
                connectionState === HubConnectionState.Connected ?
                    (
                        <PanelGroup
                            autoSaveId="example"
                            direction="horizontal">
                            <Panel
                                defaultSize={25}>
                                <DiscordTownPanel/>
                            </Panel>
                            <PanelResizeHandle
                                className="w-2 bg-gray-400 hover:bg-gray-600 cursor-col-resize"/>
                            <Panel
                                className="flex justify-center align-center">
                                <BackgroundImage
                                    gameTime={gameTime}>
                                </BackgroundImage>
                            </Panel>
                        </PanelGroup>
                    ) : (
                        <>
                            <div
                                className="text-yellow-500">{connectionState}</div>
                            <Spinner/>
                        </>
                    )}
        </>
    );
}

export default Game;