import {
    Panel,
    PanelGroup,
    PanelResizeHandle
} from "react-resizable-panels";
import {
    DiscordTownPanel,
    GameManager
} from "../components/features";
import {
    useDiscordHub
} from "../components/features/discordTownPanel/hooks/useDiscordHub.ts";
import {
    HubConnectionState
} from "@microsoft/signalr";
import {
    Spinner
} from "../components/ui";

function Home() {
    const {
        connectionState
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
                                <GameManager/>


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

export default Home;