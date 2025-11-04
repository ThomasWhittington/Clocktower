import type {
    MiniCategory
} from "@/types";
import {
    DiscordTownChannel
} from "./index.ts";

function DiscordTownCategory({category}: {
    category: MiniCategory
}) {
    const categoryId = `discord-category-${category.id}`;
    return (
        <div
            id={categoryId}>
            <p className="bg-pink-950">{category.name}</p>
            {category.channels.map(channel =>
                <DiscordTownChannel
                    key={channel.channel.id}
                    channel={channel}/>
            )}
        </div>
    );
}

export default DiscordTownCategory;