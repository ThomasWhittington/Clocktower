import type {
    MiniCategory
} from "@/types";
import {
    DiscordTownChannel
} from "./index.ts";
import {
    useState
} from "react";

function DiscordTownCategory({category}: Readonly<{
    category: MiniCategory
}>) {
    const [isOpen, setIsOpen] = useState(true);
    const toggleOpen = () => setIsOpen(prev => !prev);

    return (
        <div
            className="category-container"
            id={`discord-category-${category.id}`}
        >
            <button
                onClick={toggleOpen}
                className="unstyled-button category-header"
            >
                {isOpen ? '▾' : '▸'} {category.name}
            </button>

            {isOpen && (
                <div>
                    {category.channels.map(channel => (
                        <DiscordTownChannel
                            key={channel.channel.id}
                            channel={channel}
                        />
                    ))}
                    <br/>
                </div>
            )}
        </div>

    );
}

export default DiscordTownCategory;