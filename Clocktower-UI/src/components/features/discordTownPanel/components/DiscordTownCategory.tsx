import type {
    MiniCategory
} from "../../../../types";
import {
    DiscordTownChannel
} from "./index.ts";

function DiscordTownCategory({category}: {
    category: MiniCategory
}) {
    return (
        <>
            <p className="bg-pink-950">{category.name} {category.id}</p>
            {category.channels.map(channel =>
                <DiscordTownChannel
                    key={channel.channel.id}
                    channel={channel}/>
            )}
        </>
    );
}

export default DiscordTownCategory;