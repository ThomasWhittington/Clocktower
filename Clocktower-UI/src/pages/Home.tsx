import {
    Panel,
    PanelGroup,
    PanelResizeHandle
} from "react-resizable-panels";
import {
    DiscordTownPanel,
    GameManager
} from "../components/features";

function Home() {
    return (
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
    );
}

export default  Home;