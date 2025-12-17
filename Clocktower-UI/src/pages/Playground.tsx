import {DiscordTownPanel, TownSquare} from "@/components/features";
import {Panel, PanelGroup, PanelResizeHandle} from "react-resizable-panels";
import {BackgroundImage} from "@/components/ui";
import {Timer} from "@/components/ui/Timer.tsx";
import {GameTime} from "@/types";

export default function Playground() {
    return (
        <PanelGroup autoSaveId="playground-panel-layout" direction="horizontal">
            <Panel defaultSize={25}>
                <DiscordTownPanel/>
            </Panel>

            <PanelResizeHandle className="w-2 bg-gray-400 hover:bg-gray-600 cursor-col-resize"/>

            <Panel className="flex flex-col min-h-0">
                <BackgroundImage gameTime={GameTime.Night}>
                    <Timer/>
                    <TownSquare/>
                </BackgroundImage>
            </Panel>
        </PanelGroup>
    );
}