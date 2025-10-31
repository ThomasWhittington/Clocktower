import DiscordAdminPanel
    from "./components";
import {
    useState
} from "react";

function DiscordTownPanel() {
    const [guildId, setGuildId] = useState<bigint>();

    return (
        <div
            className="bg-[#121214] h-full">
            <DiscordAdminPanel
                guildIdChange={(id) => setGuildId(id)}/>

            {guildId &&
                <h1>{guildId}</h1>}
        </div>
    );
}

export default DiscordTownPanel;