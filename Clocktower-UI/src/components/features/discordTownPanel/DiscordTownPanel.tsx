import DiscordAdminPanel
    from "./components";
import {
    useState
} from "react";

function DiscordTownPanel() {
    const [_, setGuildId] = useState<bigint>();

    return (
        <div
            className="bg-[#121214] h-full">
            <DiscordAdminPanel
                guildIdChange={(id) => setGuildId(id)}/>
        </div>
    );
}

export default DiscordTownPanel;